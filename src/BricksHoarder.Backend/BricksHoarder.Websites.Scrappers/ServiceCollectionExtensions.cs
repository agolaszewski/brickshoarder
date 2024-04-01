using BricksHoarder.Websites.Scrappers.Lego;
using Microsoft.Extensions.DependencyInjection;

namespace BricksHoarder.Websites.Scrappers
{
    public static class ServiceCollectionExtensions
    {
        public static void AddScrappers(this IServiceCollection services)
        {
            services.AddScoped<LegoScrapper>();
        }
    }
}