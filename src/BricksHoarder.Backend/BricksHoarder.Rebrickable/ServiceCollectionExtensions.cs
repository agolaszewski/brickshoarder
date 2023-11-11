using BricksHoarder.Credentials;
using Microsoft.Extensions.DependencyInjection;
using RebrickableApi;

namespace BricksHoarder.Rebrickable
{
    public static class ServiceCollectionExtensions
    {
        public static IHttpClientBuilder AddRebrickable(this IServiceCollection services, RebrickableCredentials configuration)
        {
            return services.AddHttpClient<IRebrickableClient, RebrickableClient>(httpClient =>
            {
                httpClient.BaseAddress = configuration.Url;
                httpClient.DefaultRequestHeaders.Add("Authorization", configuration.Key);
            });
        }
    }
}