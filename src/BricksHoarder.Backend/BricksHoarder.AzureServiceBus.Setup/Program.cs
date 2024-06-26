﻿//dotnet run --project ./BricksHoarder.AzureServiceBus.Setup/BricksHoarder.AzureServiceBus.Setup.csproj

using BricksHoarder.Commands;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using BricksHoarder.Events;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

await SetupAsync();

async Task SetupAsync()
{
    var services = new ServiceCollection();

    var absolutePath = Path.GetFullPath("./BricksHoarder.Functions");

    var config = new ConfigurationBuilder()
        .SetBasePath(absolutePath)
        .AddUserSecrets<Program>()
        .AddJsonFile("production.settings.json", false)
        .Build();

    services.AddLogging(configure =>
    {
        configure.SetMinimumLevel(LogLevel.Debug);
        configure.AddConsole();
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
            .Where(t => !t.IsGenericType)
            .ToList();

        var batchedEvents = eventsAssembly
            .Where(t => typeof(IBatch).IsAssignableFrom(t))
            .Where(t => !t.IsGenericType)
            .ToList();

        x.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(azureServiceBusCredentials.ConnectionString, _ =>
            {
            });
            cfg.DeployTopologyOnly = true;
            cfg.DeployPublishTopology = true;

            cfg.Publish<IEvent>(x => x.Exclude = true);
            cfg.Publish<IBatch>(x => x.Exclude = true);
            cfg.Publish<ICommand>(x => x.Exclude = true);

            cfg.SubscriptionEndpoint("default", $"brickshoarder/fault", configure =>
            {
            });

            Thread.Sleep(1000);

            foreach (var command in commandsTypes)
            {
                cfg.ReceiveEndpoint(command.Name, configureEndpoint =>
                {
                    configureEndpoint.ConfigureConsumeTopology = false;
                    configureEndpoint.MaxDeliveryCount = 1;
                    configureEndpoint.ForwardDeadLetteredMessagesTo = "brickshoarder/fault";
                });

                cfg.SubscriptionEndpoint("default", $"brickshoarder.events/consumed/{command.Name}", configure =>
                {
                    configure.MaxDeliveryCount = 1;
                    configure.ForwardDeadLetteredMessagesTo = "brickshoarder/fault";
                });
            }

            foreach (var events in eventsTypes)
            {
                cfg.SubscriptionEndpoint("default", $"brickshoarder.events/{events.Name}", configure =>
                {
                    configure.MaxDeliveryCount = 1;
                    configure.ForwardDeadLetteredMessagesTo = "brickshoarder/fault";
                });
            }

            foreach (var events in batchedEvents)
            {
                cfg.SubscriptionEndpoint("default", $"brickshoarder.events/batch/{events.Name}", configure =>
                {
                    configure.MaxDeliveryCount = 1;
                    configure.ForwardDeadLetteredMessagesTo = "brickshoarder/fault";
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