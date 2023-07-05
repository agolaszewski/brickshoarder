using System.Data;
using System.Data.SqlClient;
using BricksHoarder.Core.Services;
using BricksHoarder.MsSql.Database.Snapshots.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.Cache.MsSql
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMsSqlSnapshotDb(this IServiceCollection services)
        {
            services.AddSingleton<SqlScripts>();
            services.AddTransient<IDbConnection, SqlConnection>(x => new SqlConnection(""));
        }
    }
}