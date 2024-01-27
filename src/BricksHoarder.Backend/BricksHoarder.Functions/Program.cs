using BricksHoarder.AzureCloud.ServiceBus;
using BricksHoarder.Common;
using BricksHoarder.Core.Services;
using BricksHoarder.Credentials;
using BricksHoarder.DateTime;
using BricksHoarder.Domain;
using BricksHoarder.Marten;
using BricksHoarder.MsSql.Database;
using BricksHoarder.MsSql.Database.Queries.CacheClean;
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

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var sqlServerDatabaseCredentials = new SqlServerDatabaseCredentials(config, "BrickshoarderDb");

        services.AddMsSqlDb(sqlServerDatabaseCredentials);
        services.AddRebrickable(new RebrickableCredentials(config));

        var redisCredentials = new RedisCredentials(new RedisLabCredentialsBase(config));
        services.AddRedis(redisCredentials);
        services.AddDomain();

        services.AddAutoMapper(mapper =>
        {
            mapper.AddDomainProfiles();
        });

        var martenCredentials = new PostgresAzureCredentials(config, "MartenAzure");
        services.AddMartenEventStore(martenCredentials);
        services.CommonServices();
        services.AddAzureServiceBusForAzureFunction(new AzureServiceBusCredentials(config, "AzureServiceBus"), redisCredentials);

        services.AddDateTimeProvider();
    })
    .Build();

var cache = host.Services.GetRequiredService<ICacheService>();
await cache.ClearAsync();

var ds = host.Services.GetRequiredService<IDocumentStore>();
await ds.Advanced.ResetAllData();

host.Run();