using BricksHoarder.Cache.InMemory;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Common;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Jobs;
using BricksHoarder.Credentials;
using BricksHoarder.Domain;
using BricksHoarder.Helpers;
using BricksHoarder.Jobs;
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
    var configuration = new RebrickableCredentials(config);
    var httpClient = new HttpClient();
    httpClient.BaseAddress = configuration.Url;
    httpClient.DefaultRequestHeaders.Add("Authorization", configuration.Key);

    return new RebrickableClient(httpClient);
});
services.AddInMemoryCache();
services.AddDomain();
services.AddJobs();

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

var handler = provider.GetService<IJob<SyncSetsJobInput>>();

await handler.RunAsync(new SyncSetsJobInput()
{
    PageNumber = 1
});

while (true)
{
    
}