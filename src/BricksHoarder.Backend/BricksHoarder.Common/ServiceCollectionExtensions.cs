using BricksHoarder.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.Common
{
    public static class ServiceCollectionExtensions
    {
        public static void CommonServices(this IServiceCollection services)
        {
            services.AddSingleton<IGuidService, GuidService>();
            services.AddSingleton<IRandomService, RandomService>();
        }
    }
}