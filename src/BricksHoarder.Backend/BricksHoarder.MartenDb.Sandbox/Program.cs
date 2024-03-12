using BricksHoarder.Azure.ServiceBus;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Common;
using BricksHoarder.Core.Commands;
using BricksHoarder.Credentials;
using BricksHoarder.Domain;
using BricksHoarder.Events;
using BricksHoarder.Redis;
using Marten;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
                .AddJsonFile("local.settings.json", false)
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
            var list = session.Events.QueryRawEventDataOnly<SetReleased>().OrderByDescending(x => x.LastModifiedDate).Take(50);

            var start = System.DateTime.UtcNow;
            
            var _sendEndpointProvider = provider.GetRequiredService<IMessageScheduler>();

            foreach (var item in list)
            {
                start = start.AddSeconds(10);
                await _sendEndpointProvider.ScheduleSend(new Uri("queue:SyncSetLegoDataCommand"), start, new SyncSetLegoDataCommand(item.SetId.Split("-")[0]));
            }
        }
    }
}