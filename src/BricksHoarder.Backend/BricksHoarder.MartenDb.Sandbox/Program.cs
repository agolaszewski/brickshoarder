using BricksHoarder.Azure.ServiceBus;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Common;
using BricksHoarder.Credentials;
using BricksHoarder.Domain;
using BricksHoarder.Events;
using BricksHoarder.Redis;
using Marten;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

//dotnet run --project ./BricksHoarder.MartenDb.Sandbox/BricksHoarder.Marten.Sandbox.csproj

namespace BricksHoarder.Marten.Sandbox
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            var absolutePath = Path.GetFullPath("C:\\Users\\Andrzej\\source\\repos\\agolaszewski\\brickshoarder\\src\\BricksHoarder.Backend\\BricksHoarder.Functions");

            var config = new ConfigurationBuilder()
                .SetBasePath(absolutePath)
                .AddUserSecrets<Program>()
                .AddJsonFile("production.settings.json", false)
                .Build();

            services.AddLogging(configure =>
            {
                configure.SetMinimumLevel(LogLevel.Debug);
                configure.AddConsole();
            });

            var postgresCredentials = new PostgresCredentials(config, "MartenAzure");

            services.AddMartenEventStore(postgresCredentials);

            var redisCredentials = new RedisCredentials(new RedisLabCredentialsBase(config));
            services.AddRedis(redisCredentials);

            services.CommonServices();
            services.AddAutoMapper(mapper =>
            {
                mapper.AddDomainProfiles();
            });
            services.AddAzureServiceBusForAzureFunction(new AzureServiceBusCredentials(config, "AzureServiceBus"), redisCredentials);

            var provider = services.BuildServiceProvider();

            var store = provider.GetRequiredService<IDocumentStore>();

            //using var daemon = await store.BuildProjectionDaemonAsync();
            //await daemon.RebuildProjection<TestTransformation>(10.Minutes(), CancellationToken.None);

            var session = store.LightweightSession();
            var list = session.Events
                .QueryAllRawEvents()
                .Where(x => x.DotNetTypeName == "BricksHoarder.Events.NewLegoSetDiscovered, BricksHoarder.Events")
                .OrderByDescending(x => x.Timestamp).ToList()
                .Select(x => x.Data as NewLegoSetDiscovered)
                .Where(x => x.Availability != LegoSetAvailability.Discontinued)
                .Where(x => x.SetId.Contains("-"))
                .ToList();

            var _sendEndpointProvider = provider.GetRequiredService<IMessageScheduler>();

            var r = new RandomService();
            var now = System.DateTime.Now;
            var start = now.Date.AddDays(0).AddHours(14).ToUniversalTime();
            var end = now.Date.AddDays(0).AddHours(16).ToUniversalTime();

            foreach (var item in list)
            {
                var schedule = r.Between(start, end);
                await _sendEndpointProvider.ScheduleSend(new Uri("queue:SyncSetLegoDataCommand"), schedule, new SyncSetLegoDataCommand(item.SetId), Pipe.Execute<SendContext<SyncSetLegoDataCommand>>(x => x.CorrelationId = Guid.NewGuid()));
            }
        }
    }
}