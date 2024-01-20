using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace BricksHoarder.Playwright
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPlaywright(this IServiceCollection services)
        {
            services.AddScoped<IPageFactory,ProductionPageFactory>();
            services.AddSingleton<CookiesFactory>();
        }
    }
}