using BricksHoarder.AzureCloud.ServiceBus;
using BricksHoarder.Common;
using BricksHoarder.Core.Services;
using BricksHoarder.Credentials;
using BricksHoarder.DateTime;
using BricksHoarder.Domain;
using BricksHoarder.Marten;
using BricksHoarder.Rebrickable;
using BricksHoarder.Redis;
using Marten;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: false);
    })
    .ConfigureServices((builder, services) =>
    {
        var config = builder.Configuration;

        if (builder.HostingEnvironment.IsDevelopment())
        {
            Development(services, config);
        }
        else
        {
            Production(services, config);
        }
    })
    .Build();

void Production(IServiceCollection services, IConfiguration config)
{
    Common(services, config);

    services.AddApplicationInsightsTelemetryWorkerService();
    services.ConfigureFunctionsApplicationInsights();

    var martenCredentials = new PostgresAzureCredentials(config, "MartenAzure");
    services.AddMartenEventStore(martenCredentials);
}

void Development(IServiceCollection services, IConfiguration config)
{
    Common(services, config);

    var martenCredentials = new PostgresCredentials(config, "MartenAzure");
    services.AddMartenEventStore(martenCredentials);
}

void Common(IServiceCollection services, IConfiguration config)
{
    services.AddDomain();
    services.AddAutoMapper(mapper =>
    {
        mapper.AddDomainProfiles();
    });

    services.CommonServices();
    services.AddDateTimeProvider();

    services.AddRebrickable(new RebrickableCredentials(config));

    var redisCredentials = new RedisCredentials(new RedisLabCredentialsBase(config));
    services.AddRedis(redisCredentials);

    services.AddAzureServiceBusForAzureFunction(new AzureServiceBusCredentials(config, "AzureServiceBus"), redisCredentials);
}

var cache = host.Services.GetRequiredService<ICacheService>();
await cache.ClearAsync();

var ds = host.Services.GetRequiredService<IDocumentStore>();
await ds.Advanced.ResetAllData();

host.Run();