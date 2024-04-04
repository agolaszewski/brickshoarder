using BricksHoarder.Azure.ServiceBus;
using BricksHoarder.Common;
using BricksHoarder.Credentials;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Domain;
using BricksHoarder.Marten;
using BricksHoarder.Playwright;
using BricksHoarder.Rebrickable;
using BricksHoarder.Redis;
using BricksHoarder.Serilog;
using BricksHoarder.Websites.Scrappers;
using Marten;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
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

    Log.Logger = Log.Logger.AddSerilog().AddSeq(new Uri("http://localhost:5341/")).CreateLogger();

    services.AddLogging(lb => lb.AddSerilog(Log.Logger, true));

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

    services.AddScrappers();
    services.AddPlaywright();


    services.AddAzureServiceBusForAzureFunction(new AzureServiceBusCredentials(config, "AzureServiceBus"), redisCredentials);
}

IWebHostEnvironment env = host.Services.GetRequiredService<IWebHostEnvironment>();

if (env.IsDevelopment())
{
    //var cache = host.Services.GetRequiredService<ICacheService>();
    //await cache.ClearAsync();

    //var ds = host.Services.GetRequiredService<IDocumentStore>();
    //await ds.Advanced.ResetAllData();
}

await host.RunAsync();