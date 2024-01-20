using BricksHoarder.AzureCloud.ServiceBus;
using BricksHoarder.Cache.MsSql;
using BricksHoarder.Cache.NoCache;
using BricksHoarder.Common;
using BricksHoarder.Credentials;
using BricksHoarder.DateTime;
using BricksHoarder.Domain;
using BricksHoarder.Marten;
using BricksHoarder.MsSql.Database;
using BricksHoarder.MsSql.Database.Queries.CacheClean;
using BricksHoarder.Rebrickable;
using Marten;
using MassTransit;
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
        services.AddNoCache();
        services.AddDomain();

        services.AddAutoMapper(mapper =>
        {
            mapper.AddDomainProfiles();
        });

        var martenCredentials = new PostgresAzureCredentials(config, "MartenAzure");
        services.AddMartenEventStore(martenCredentials);
        services.CommonServices();
        services.AddAzureServiceBusForAzureFunction(new AzureServiceBusCredentials(config, "AzureServiceBus"));

        services.AddDateTimeProvider();
    })
    .Build();

var cacheClean = host.Services.GetRequiredService<CleanCache>();
await cacheClean.ExecuteAsync();

var ds = host.Services.GetRequiredService<IDocumentStore>();
await ds.Advanced.ResetAllData();

host.Run();
