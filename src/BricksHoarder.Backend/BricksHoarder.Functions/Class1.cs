using BricksHoarder.Cache.InMemory;
using BricksHoarder.Common;
using BricksHoarder.Credentials;
using BricksHoarder.Domain;
using BricksHoarder.Marten;
using BricksHoarder.RabbitMq;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RebrickableApi;
using System.IO;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(BricksHoarder.Functions.Startup))]

namespace BricksHoarder.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();
            builder.ConfigurationBuilder.Sources.Clear();

            builder.ConfigurationBuilder
               .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: false, reloadOnChange: true)
               .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
               .AddEnvironmentVariables();
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

            services.AddInMemoryCache();
            services.AddDomain();

            services.AddAutoMapper(config =>
            {
                config.AddDomainProfiles();
            });

            services.AddMartenEventStore(new PostgresAzureCredentials(config, "MartenAzure"));
            services.CommonServices();
            services.AddAzureServiceBusForAzureFunction(new AzureServiceBusCredentials(config, "AzureServiceBus"));
        }
    }
}