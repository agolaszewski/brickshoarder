using BricksHoarder.AzureServiceBus;
using BricksHoarder.Cache.MsSql;
using BricksHoarder.Common;
using BricksHoarder.Credentials;
using BricksHoarder.DateTime;
using BricksHoarder.Domain;
using BricksHoarder.Marten;
using BricksHoarder.MsSql.Database;
using BricksHoarder.Rebrickable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("local.settings.json", optional: false, reloadOnChange: false);
    })
    .ConfigureServices((builder, services) =>
    {
        var config = builder.Configuration;

        var sqlServerDatabaseCredentials = new SqlServerDatabaseCredentials(config, "BrickshoarderDb");

        services.AddMsSqlDb(sqlServerDatabaseCredentials);
        services.AddRebrickable(new RebrickableCredentials(config));

        services.AddMsSqlAsCache(sqlServerDatabaseCredentials);
        services.AddDomain();

        services.AddAutoMapper(mapper =>
        {
            mapper.AddDomainProfiles();
        });

        var martenCredentials = new PostgresAzureCredentials(config, "MartenAzure");
        services.AddMartenEventStore(martenCredentials);
        services.CommonServices();
        services.AddAzureServiceBusForAzureFunction(new AzureServiceBusCredentials(config, "AzureServiceBus"), sqlServerDatabaseCredentials);
        services.AddDateTimeProvider();
    })
    .Build();

host.Run();