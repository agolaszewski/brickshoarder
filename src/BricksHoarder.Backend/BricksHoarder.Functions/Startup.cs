using BricksHoarder.AzureServiceBus;
using BricksHoarder.Cache.NoCache;
using BricksHoarder.Common;
using BricksHoarder.Credentials;
using BricksHoarder.Domain;
using BricksHoarder.Marten;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RebrickableApi;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(BricksHoarder.Functions.Startup))]

namespace BricksHoarder.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();
            builder.ConfigurationBuilder.SetBasePath(context.ApplicationRootPath);
            builder.ConfigurationBuilder.AddJsonFile("local.settings.json", optional: false, reloadOnChange: false);
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;
            var config = builder.GetContext().Configuration;

            builder.Services.AddHttpClient();
            services.AddSingleton<IRebrickableClient>(services =>
            {
                var configuration = new RebrickableCredentials(config);
                var httpClient = services.GetRequiredService<HttpClient>();
                httpClient.BaseAddress = configuration.Url;
                httpClient.DefaultRequestHeaders.Add("Authorization", configuration.Key);

                return new RebrickableClient(httpClient);
            });

            services.AddNoCache();
            services.AddDomain();

            services.AddAutoMapper(mapper =>
            {
                mapper.AddDomainProfiles();
            });

            var martenCredentials = new PostgresAzureCredentials(config, "MartenAzure");
            services.AddMartenEventStore(martenCredentials);
            services.CommonServices();
            services.AddAzureServiceBusForAzureFunction(new AzureServiceBusCredentials(config, "AzureServiceBus"), martenCredentials);
        }
    }
}