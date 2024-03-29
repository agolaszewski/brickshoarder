﻿using BricksHoarder.AzureServiceBus;
using BricksHoarder.Cache.NoCache;
using BricksHoarder.Common;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using BricksHoarder.Domain;
using BricksHoarder.Marten;
using BricksHoarder.Redis;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RebrickableApi;
using System.CommandLine;
using Environment = BricksHoarder.Local.Runner.Environment;

var rootCommand = new RootCommand("Local runner");
var azureOption = new Option<bool>("--azure");
rootCommand.Add(azureOption);

rootCommand.SetHandler(async (azureOptionValue) =>
{
    var env = Environment.Azure;
    await SetupAsync(env);
},
azureOption);

rootCommand.Invoke(args);

//while (true)
//{
//}

async Task SetupAsync(Environment env)
{
    var services = new ServiceCollection();
    var config = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .Build();

    services.AddLogging(config =>
    {
        config.SetMinimumLevel(LogLevel.Debug);
        config.AddConsole();
    });

    services.AddSingleton<IRebrickableClient>(_ =>
    {
        var configuration = new RebrickableCredentials(config);
        var httpClient = new HttpClient();
        httpClient.BaseAddress = configuration.Url;
        httpClient.DefaultRequestHeaders.Add("Authorization", configuration.Key);

        return new RebrickableClient(httpClient);
    });

    services.AddDomain();
    services.AddAutoMapper(config => { config.AddDomainProfiles(); });

    switch (env)
    {
        case Environment.Azure:
            services.AddNoCache();
            services.AddMartenEventStore(new PostgresAzureCredentials(config, "MartenAzure"));
            services.AddAzureServiceBus(new AzureServiceBusCredentials(config, "AzureServiceBus"));
            break;

        default:
            services.AddRedis(new RedisCredentials(new RedisLocalCredentialsBase(config)));
            services.AddMartenEventStore(new PostgresCredentials(config, "Marten"));
            break;
    }

    services.CommonServices();

    var provider = services.BuildServiceProvider();
    var bus = provider.GetRequiredService<IBusControl>();
    await bus.StartAsync();

    var dispatcher = provider.GetRequiredService<IEventDispatcher>();
    //await dispatcher.DispatchAsync(new SyncSetsSaga(DateTime.Today.ToString("yyyyMMdd")));
}