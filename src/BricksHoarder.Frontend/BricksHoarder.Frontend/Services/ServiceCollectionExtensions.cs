using BricksHoarder.Abstraction;
using BricksHoarder.Abstraction.WishListService;

namespace BricksHoarder.Frontend.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IWishListService, WishListService>();
            return services;
        }
    }
}
