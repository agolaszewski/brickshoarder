using BricksHoarder.Core.Credentials;
using BricksHoarder.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.SqlClient;

namespace BricksHoarder.Cache.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMsSqlMemoryCache(this IServiceCollection services, IConnectionString connectionString)
        {
            services.AddTransient<IDbConnection, SqlConnection>(_ => new SqlConnection(connectionString.ConnectionString));
            services.AddScoped<ICacheService, MsSqlCache>();
        }
    }
}