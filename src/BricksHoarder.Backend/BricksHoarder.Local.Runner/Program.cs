using BricksHoarder.Cache.InMemory;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Common;
using BricksHoarder.Core.Commands;
using BricksHoarder.Credentials;
using BricksHoarder.Domain;
using BricksHoarder.Helpers;
using BricksHoarder.Marten;
using BricksHoarder.RabbitMq;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RebrickableApi;

var services = new ServiceCollection();
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

services.AddLogging(config =>
{
    config.SetMinimumLevel(LogLevel.Debug);
    config.AddConsole();
});

services.AddSingleton<IRebrickableClient>(_ =>
{
    var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(config.Get("Rebrickable:Url"));
    httpClient.DefaultRequestHeaders.Add("Authorization", $"key {config.Get("Rebrickable:Key")}");

    return new RebrickableClient(httpClient);
});
services.AddInMemoryCache();
services.AddDomain();
services.AddAutoMapper(config =>
{
    config.AddDomainProfiles();
});

services.AddMartenEventStore(new PostgresCredentials(config, "Marten"));
services.AddDispatcher(new RabbitMqCredentials(config));
services.CommonServices();

var provider = services.BuildServiceProvider();
var bus = provider.GetService<IBusControl>();
await bus.StartAsync();

var handler = provider.GetService<ICommandDispatcher>();

await handler.DispatchAsync(new CreateSetCommand("000", "123", 2022, 1, 1, null, DateTime.Today));

while (true)
{
    
}