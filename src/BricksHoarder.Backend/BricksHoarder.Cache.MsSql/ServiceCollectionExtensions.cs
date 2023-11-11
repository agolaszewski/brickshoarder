using BricksHoarder.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.Cache.MsSql
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMsSqlAsCache(this IServiceCollection services)
        {
            services.AddScoped<ICacheService, MsSqlCache>();
        }
    }
}