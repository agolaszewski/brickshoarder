using BricksHoarder.Credentials;
using MassTransit.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.MassTransit
{
    public static class ServiceCollectionExtensions
    {
        public static void AddOutbox(this IServiceCollection services, SqlServerDatabaseCredentials credentials)
        {
            services.AddHostedService<RecreateDatabaseHostedService<RegistrationDbContext>>();
            services.AddDbContext<MassTransitDbContext>(x =>
            {
                x.UseSqlServer(credentials.ConnectionString, opt =>
                {
                    //opt.EnableRetryOnFailure();
                    //opt.CommandTimeout(30);
                });
            });
        }
    }
}