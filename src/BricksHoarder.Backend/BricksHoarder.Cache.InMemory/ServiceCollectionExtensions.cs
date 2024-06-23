using BricksHoarder.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.Cache.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInMemoryCache(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddScoped<ICacheService, InMemoryCache>();
        }
    }
}