using BricksHoarder.Core.Aggregates;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

namespace RealWorld.EventStore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEventStore(this IServiceCollection services, Uri address)
        {
            services.AddSingleton(b =>
            {
                var settings = new EventStoreClientSettings
                {
                    ConnectivitySettings = {
                        Address = address
                    }
                };

                return new EventStoreClient(settings);
            });
            services.AddScoped<IAggregateStore, EventStoreAggregateStore>();
        }
    }
}