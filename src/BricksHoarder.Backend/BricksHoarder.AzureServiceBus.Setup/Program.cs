using BricksHoarder.Commands;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

await SetupAsync();

async Task SetupAsync()
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

    var azureServiceBusCredentials = new AzureServiceBusCredentials(config, "AzureServiceBus");

    services.AddMassTransit(x =>
    {
        x.SetKebabCaseEndpointNameFormatter();

        var commandsAssembly = typeof(BricksHoarderCommandsAssemblyPointer).Assembly.GetTypes();
        var eventsAssembly = typeof(BricksHoarderEventsAssemblyPointer).Assembly.GetTypes();

        var commandsTypes = commandsAssembly
            .Where(t => typeof(ICommand).IsAssignableFrom(t))
            .ToList();

        var eventsTypes = eventsAssembly
            .Where(t => typeof(IEvent).IsAssignableFrom(t))
            .ToList();

        x.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(azureServiceBusCredentials.ConnectionString, _ =>
            {
            });
            cfg.DeployTopologyOnly = true;

            cfg.Publish<IEvent>(x => x.Exclude = true);
            cfg.Publish<ICommand>(x => x.Exclude = true);

            foreach (var command in commandsTypes)
            {
                cfg.ReceiveEndpoint(command.Name, configureEndpoint =>
                {
                    configureEndpoint.ConfigureConsumeTopology = false;
                });
            }

            foreach (var events in commandsTypes)
            {
                cfg.SubscriptionEndpoint("default", $"brickshoarder.events/{events.Name.ToLower()}", _ =>
                {

                });
            }

            cfg.ConfigureEndpoints(context);
        });
    });

    var provider = services.BuildServiceProvider();
    var bus = provider.GetRequiredService<IBusControl>();

    Console.WriteLine("Deploying Topology");
    using var source = new CancellationTokenSource(TimeSpan.FromMinutes(2));
    await bus.DeployAsync(source.Token);
    Console.WriteLine("Topology Deployed");
}