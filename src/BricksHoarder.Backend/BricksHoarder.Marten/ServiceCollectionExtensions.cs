using BricksHoarder.Core.Aggregates;
using BricksHoarder.Credentials;
using BricksHoarder.Projections;
using Marten;
using Marten.Events;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;
using Weasel.Core;

namespace BricksHoarder.Marten
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMartenEventStore(this IServiceCollection services, PostgresCredentials credentials)
        {
            AddMartenEventStore(services, credentials.ConnectionString);
        }

        public static void AddMartenEventStore(this IServiceCollection services, PostgresAzureCredentials credentials)
        {
            AddMartenEventStore(services, credentials.ConnectionString);
        }

        private static void AddMartenEventStore(IServiceCollection services, string connectionString)
        {
            services.AddMarten(options =>
            {
                options.Connection(connectionString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.Events.StreamIdentity = StreamIdentity.AsString;

                options.Projections.Add<TestTransformation>(ProjectionLifecycle.Inline);
                options.Events.Subscribe(new TestSubscription());
            })
            .AddAsyncDaemon(DaemonMode.HotCold);

            services.AddScoped<IAggregateStore, MartenAggregateStore>();
        }
    }
}