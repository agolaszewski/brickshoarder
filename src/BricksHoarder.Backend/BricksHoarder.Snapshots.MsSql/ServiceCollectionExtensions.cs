using BricksHoarder.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.SqlClient;

namespace BricksHoarder.Cache.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMsSqlMemoryCache(this IServiceCollection services)
        {
            services.AddTransient<IDbConnection, SqlConnection>(x => new SqlConnection(""));
            services.AddScoped<ICacheService, MsSqlCache>();
        }
    }
}