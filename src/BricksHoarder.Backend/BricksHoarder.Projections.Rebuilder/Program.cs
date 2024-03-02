using BricksHoarder.Credentials;
using BricksHoarder.Marten;
using JasperFx.Core;
using Marten;
using Marten.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weasel.Core;

namespace BricksHoarder.Projections.Rebuilder
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            var absolutePath = Path.GetFullPath("./BricksHoarder.Functions");

            var config = new ConfigurationBuilder()
                //.SetBasePath(absolutePath)
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

            var provider = services.BuildServiceProvider();

            var store = provider.GetRequiredService<IDocumentStore>();

            //using var daemon = await store.BuildProjectionDaemonAsync();
            //await daemon.RebuildProjection<TestTransformation>(10.Minutes(), CancellationToken.None);

            var session = store.LightweightSession();
            var xd = session.Json.FindById<Test>("RebrickableSetAggregate:76284-1");
        }
    }
}