using BricksHoarder.Core.Services;
using BricksHoarder.Credentials;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.Cache.MsSql
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMsSqlAsCache(this IServiceCollection services, SqlServerDatabaseCredentials credentials)
        {
            services.AddDistributedSqlServerCache(config =>
            {
                config.ConnectionString = credentials.ConnectionString;
                config.TableName = "_Cache";
                config.SchemaName = "dbo";
            });

            services.AddScoped<ICacheService, MsSqlCache>();
        }
    }
}