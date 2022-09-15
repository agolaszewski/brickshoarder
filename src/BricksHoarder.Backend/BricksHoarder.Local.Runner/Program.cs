using BricksHoarder.Cache.NoCache;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Common;
using BricksHoarder.Core.Commands;
using BricksHoarder.Credentials;
using BricksHoarder.Domain;
using BricksHoarder.Marten;
using BricksHoarder.RabbitMq;
using BricksHoarder.Redis;
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
//services.AddRedis(new RedisCredentials(new RedisLocalCredentialsBase(config)));
services.AddNoCache();
services.AddDomain();

services.AddAutoMapper(config =>
{
    config.AddDomainProfiles();
});

//services.AddMartenEventStore(new PostgresCredentials(config, "Marten"));
services.AddMartenEventStore(new PostgresAzureCredentials(config, "MartenAzure"));
services.CommonServices();
//services.AddRabbitMq(new RabbitMqCredentials(config));
services.AddAzureServiceBus(new AzureServiceBusCredentials(config, "AzureServiceBus"));

var provider = services.BuildServiceProvider();
var bus = provider.GetService<IBusControl>();
await bus.StartAsync();

var dispatcher = provider.GetService<ICommandDispatcher>();
await dispatcher.DispatchAsync(new SyncThemesCommand()
{
    CorrelationId = Guid.Empty
});
//await dispatcher!.DispatchAsync(new CreateSetCommand()
//{
//   Name = "ASda",
//   Year = 231,
//   ThemeId = 1,
//   CorrelationId = Guid.Empty,
//   ImageUrl = null,
//   LastModifiedDate = DateTime.UtcNow,
//   NumberOfParts = 2,
//   SetNumber = "441"
//});

while (true)
{
}