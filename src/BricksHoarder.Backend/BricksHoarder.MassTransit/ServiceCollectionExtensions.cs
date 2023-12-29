using BricksHoarder.Credentials;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.MassTransit
{
    //
    public static class ServiceCollectionExtensions
    {
        public static void AddOutbox(this IServiceCollection services, SqlServerDatabaseCredentials credentials)
        {
            services.AddDbContext<MassTransitDbContext>(x =>
            {
                x.UseSqlServer(credentials.ConnectionString, opt =>
                {
                    //opt.MigrationsAssembly("BricksHoarder.MassTransit");
                    //opt.EnableRetryOnFailure();
                    //opt.CommandTimeout(30);
                });
            });
        }
    }
}