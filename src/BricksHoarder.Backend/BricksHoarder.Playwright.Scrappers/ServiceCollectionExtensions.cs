using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.Playwright
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPlaywright(this IServiceCollection services)
        {
            services.AddScoped<IPageFactory, ProductionPageFactory>();
            services.AddSingleton<CookiesFactory>();
        }
    }
}