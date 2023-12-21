using BricksHoarder.Credentials;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using RebrickableApi;
using ThrottlingTroll;

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
            })
            .AddThrottlingTrollMessageHandler(options =>
            {
                options.Config = new ThrottlingTrollEgressConfig()
                {
                    Rules = new List<ThrottlingTrollRule>()
                    {
                        new()
                        {
                            LimitMethod = new FixedWindowRateLimitMethod
                            {
                                PermitLimit = 1,
                                IntervalInSeconds = 5
                            },
                            MaxDelayInSeconds = 60,
                        }
                    },
                    UniqueName = "Rebrickable"
                };
            })
            .AddPolicyHandler(GetHandleTransientHttpError());
        }

        private static IAsyncPolicy<HttpResponseMessage> GetHandleTransientHttpError()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}