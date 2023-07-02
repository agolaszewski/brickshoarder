using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using BricksHoarder.Domain.Sets;
using BricksHoarder.Events;
using MassTransit;
using MassTransit.ServiceBusIntegration;
using MassTransit.Testing;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BricksHoarder.AzureServiceBus
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAzureServiceBus(this IServiceCollection services, AzureServiceBusCredentials credentials)
        {
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<RequestToCommandMapper>();
            services.AddScoped<IIntegrationEventsQueue, IntegrationEventsQueue>();

            services.AddMassTransit(x =>
            {
                var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Single(assembly => assembly.GetName().Name == "BricksHoarder.Domain").GetTypes();

                var eventsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Single(assembly => assembly.GetName().Name == "BricksHoarder.Events").GetTypes();

                var commandsHandlersTypes = domainAssembly
                    .Where(t => t.IsNested && t.Name == "Handler")
                    .Select(t => t.GetInterfaces().First())
                    .Where(t => typeof(ICommandHandler<>).IsAssignableFrom(t.GetGenericTypeDefinition()))
                    .ToList();

                foreach (var commandHandlerType in commandsHandlersTypes)
                {
                    var typeArguments = commandHandlerType.GetGenericArguments();
                    x.AddConsumer(typeof(CommandConsumer<>).MakeGenericType(typeArguments));
                }

                var sagas = domainAssembly
                    .Where(t => t.Name.EndsWith("Saga"));

                x.AddSagaStateMachine<SyncSetsSaga, SyncSetsSagaState>().InMemoryRepository();

                //foreach (var sagaType in sagas)
                //{
                //    x.AddSagaStateMachine(sagaType);
                //}
                //x.SetInMemorySagaRepositoryProvider();

                var events = eventsAssembly
                    .Where(t => t.GetInterface(nameof(IEvent)) is not null)
                    .ToList();

                foreach (var eventType in events)
                {
                    //x.AddConsumer(typeof(EventConsumer<>).MakeGenericType(eventType));
                }

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    var options = context.GetRequiredService<IOptions<ServiceBusOptions>>();
                    options.Value.AutoCompleteMessages = true;

                    cfg.Host(credentials.ConnectionString, _ =>
                    {
                    });

                    cfg.Publish<IEvent>(x => x.Exclude = true);
                    cfg.Publish<ICommand>(x => x.Exclude = true);

                    cfg.Message<CommandConsumed<SyncThemesCommand>>(x =>
                    {
                        x.SetEntityName("brickshoarder.events/consumed/syncthemescommand");
                    });

                    cfg.UseServiceBusMessageScheduler();
                });
            });
        }

        public static void AddAzureServiceBusForAzureFunction(this IServiceCollection services, AzureServiceBusCredentials credentials, PostgresAzureCredentials postgresAzureCredentials)
        {
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<IEventDispatcher, EventDispatcher>();
            services.AddSingleton<IMessageReceiver, MessageReceiver>();
            services.AddSingleton<IAsyncBusHandle, AsyncBusHandle>();
            services.AddScoped<IIntegrationEventsQueue, IntegrationEventsQueue>();

            services.AddMassTransit(x =>
            {
                var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Single(assembly => assembly.GetName().Name == "BricksHoarder.Domain").GetTypes();

                var eventsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Single(assembly => assembly.GetName().Name == "BricksHoarder.Events").GetTypes();

                var commandsHandlersTypes = domainAssembly
                    .Where(t => t.IsNested && t.Name == "Handler")
                    .Select(t => t.GetInterfaces().First())
                    .Where(t => typeof(ICommandHandler<>).IsAssignableFrom(t.GetGenericTypeDefinition()))
                    .ToList();

                foreach (var commandHandlerType in commandsHandlersTypes)
                {
                    var typeArguments = commandHandlerType.GetGenericArguments();
                    x.AddConsumer(typeof(CommandConsumer<>).MakeGenericType(typeArguments));
                }

                var sagas = domainAssembly
                    .Where(t => t.Name.EndsWith("Saga"));

                x.AddSagaStateMachine<SyncSetsSaga, SyncSetsSagaState>().InMemoryRepository();

                //foreach (var sagaType in sagas)
                //{
                //    x.AddSagaStateMachine(sagaType);
                //}
                //x.SetInMemorySagaRepositoryProvider();

                var events = eventsAssembly
                    .Where(t => t.GetInterface(nameof(IEvent)) is not null)
                    .ToList();

                foreach (var eventType in events)
                {
                    //x.AddConsumer(typeof(EventConsumer<>).MakeGenericType(eventType));
                }

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    var options = context.GetRequiredService<IOptions<ServiceBusOptions>>();
                    options.Value.AutoCompleteMessages = true;

                    cfg.Host(credentials.ConnectionString, _ =>
                    {
                    });

                    cfg.Publish<IEvent>(x => x.Exclude = true);
                    cfg.Publish<ICommand>(x => x.Exclude = true);

                    cfg.Message<CommandConsumed<SyncThemesCommand>>(x =>
                    {
                        x.SetEntityName("brickshoarder.events/consumed/SyncThemesCommand");
                    });

                    cfg.Message<CommandConsumed<SyncSetsByThemeCommand>>(x =>
                    {
                        x.SetEntityName("brickshoarder.events/consumed/SyncSetsByThemeCommand");
                    });

                    cfg.UseServiceBusMessageScheduler();
                });
            });

            services.RemoveMassTransitHostedService();
        }
    }
}