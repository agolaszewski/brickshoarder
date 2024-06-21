using BricksHoarder.Azure.ServiceBus;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Common;
using BricksHoarder.Common.DDD.Exceptions;
using BricksHoarder.Credentials;
using BricksHoarder.DateTime;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Domain.LegoSet;
using BricksHoarder.Domain.RebrickableSet;
using BricksHoarder.Domain.SetsCollection;
using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Domain.ThemesCollection;
using BricksHoarder.Events;
using BricksHoarder.Redis;
using BricksHoarder.Serilog;
using MassTransit;
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
}

void Development(IServiceCollection services, IConfiguration config)
{
    Common(services, config);

    Log.Logger = Log.Logger.AddSerilog().AddSeq(new Uri("http://localhost:5341/")).CreateLogger();
    services.AddLogging(lb => lb.AddSerilog(Log.Logger, true));
}

void Common(IServiceCollection services, IConfiguration config)
{
    services.CommonServices();
    services.AddDateTimeProvider();

    //services.AddAzureServiceBusForAzureFunction(new AzureServiceBusCredentials(config, "AzureServiceBus"), bus =>
    //{
    //},
    //(context, cfg) =>
    //{
    //});
}

host.Run();