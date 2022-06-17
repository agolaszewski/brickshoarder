using BricksHoarder.Core.Aggregates;
using BricksHoarder.Credentials;
using Marten;
using Marten.Events;
using Microsoft.Extensions.DependencyInjection;
using Weasel.Core;

namespace BricksHoarder.Marten
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMartenEventStore(this IServiceCollection services, PostgresCredentials credentials)
        {
            services.AddMarten(options =>
            {
                options.Connection(credentials.ConnectionString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.Events.StreamIdentity = StreamIdentity.AsString;
            });
            services.AddScoped<IAggregateStore, MartenAggregateStore>();
        }
    }
}