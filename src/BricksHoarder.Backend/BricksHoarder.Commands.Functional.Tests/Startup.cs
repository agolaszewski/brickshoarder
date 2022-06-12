using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BricksHoarder.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RebrickableApi;

namespace BricksHoarder.Commands.Functional.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Startup>()
                .Build();

            services.AddSingleton<RebrickableClient>(_ =>
            {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("https://rebrickable.com");
                httpClient.DefaultRequestHeaders.Add("Authorization", $"key {config["RebrickableApi:Key"]}");

                return new RebrickableClient(httpClient);
            });
            services.AddMartenEventStore(null);
        }
    }
}
