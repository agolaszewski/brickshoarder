using BricksHoarder.Cache.InMemory;
using BricksHoarder.Domain;
using BricksHoarder.Marten;
using Microsoft.Extensions.DependencyInjection;
using RebrickableApi;
using System;
using System.Net.Http;

namespace BricksHoarder.Commands.Functional.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<RebrickableClient>(_ =>
            {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("https://rebrickable.com");
                httpClient.DefaultRequestHeaders.Add("Authorization", $"key x");

                return new RebrickableClient(httpClient);
            });
            services.AddInMemoryCache();
            services.AddDomain();

            services.AddMartenEventStore(null);
        }
    }
}