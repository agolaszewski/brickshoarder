using BricksHoarder.AzureServiceBus;
using BricksHoarder.Cache.NoCache;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Common;
using BricksHoarder.Core.Commands;
using BricksHoarder.Credentials;
using BricksHoarder.Domain;
using BricksHoarder.Marten;
using BricksHoarder.RabbitMq;
using BricksHoarder.Redis;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RebrickableApi;
using System.CommandLine;

var rootCommand = new RootCommand("Local runner");
var azureOption = new Option<bool>("--azure");
rootCommand.Add(azureOption);

rootCommand.SetHandler(async (azureOptionValue) =>
{
    var env = azureOptionValue ? Environment.Azure : Environment.Local;
    await SetupAsync(env);
},
azureOption);

rootCommand.Invoke(args);

while (true)
{
}

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
            services.AddRabbitMq(new RabbitMqCredentials(config));
            services.AddMartenEventStore(new PostgresCredentials(config, "Marten"));
            break;
    }

    services.CommonServices();

    var provider = services.BuildServiceProvider();
    var bus = provider.GetRequiredService<IBusControl>();
    await bus.StartAsync();

    var dispatcher = provider.GetRequiredService<ICommandDispatcher>();
    await dispatcher.DispatchAsync(new SyncThemesCommand());
}