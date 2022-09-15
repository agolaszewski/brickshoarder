using BricksHoarder.Cache.InMemory;
using BricksHoarder.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.Cache.NoCache
{
    public static class ServiceCollectionExtensions
    {
        public static void AddNoCache(this IServiceCollection services)
        {
            services.AddScoped<ICacheService, BricksHoarder.Cache.InMemory.NoCache>();
        }
    }
}