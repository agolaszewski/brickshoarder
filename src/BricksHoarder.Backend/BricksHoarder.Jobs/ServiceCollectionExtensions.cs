using BricksHoarder.Core.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static void AddJobs(this IServiceCollection services)
        {
            var jobsAssembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "BricksHoarder.Jobs");

            services.Scan(scan =>
                scan.FromAssemblies(jobsAssembly!)
                    .AddClasses(classes => classes.AssignableTo(typeof(IJob<>)))
                    .AsImplementedInterfaces().WithTransientLifetime()
            );
        }
    }
}