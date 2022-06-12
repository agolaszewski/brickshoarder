using BricksHoarder.Core.Aggregates;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Weasel.Core;

namespace BricksHoarder.Marten
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMartenEventStore(this IServiceCollection services, Uri address)
        {
            services.AddMarten(options =>
            {
                options.Connection(address.OriginalString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
            });
            services.AddScoped<IAggregateStore, MartenAggregateStore>();
        }
    }
}