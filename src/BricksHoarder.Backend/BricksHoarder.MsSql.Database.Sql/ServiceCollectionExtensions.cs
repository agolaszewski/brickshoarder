using BricksHoarder.Core.Database;
using BricksHoarder.Credentials;
using BricksHoarder.MsSql.Database.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.SqlClient;

namespace BricksHoarder.MsSql.Database
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMsSqlDb(this IServiceCollection services, SqlServerDatabaseCredentials credentials)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "BricksHoarder.MsSql.Database");

            services.AddSingleton<SqlScripts>();
            services.AddTransient<IDbConnection, SqlConnection>(x => new SqlConnection(credentials.ConnectionString));

            services.Scan(scan =>
                scan.FromAssemblies(assembly!)
                    .AddClasses(classes => classes.AssignableTo(typeof(IDbQuery)))
                    .AsSelfWithInterfaces().WithSingletonLifetime()
            );

            services.AddTransient<IDbConnection, SqlConnection>(_ => new SqlConnection(credentials.ConnectionString));
        }
    }
}