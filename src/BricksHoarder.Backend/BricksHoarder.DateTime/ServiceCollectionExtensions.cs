using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.DateTime
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDateTimeProvider(this IServiceCollection services)
        {
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        }
    }
}