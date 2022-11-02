using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using MassTransit;
using MassTransit.ServiceBusIntegration;
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
                    .SingleOrDefault(assembly => assembly.GetName().Name == "BricksHoarder.Domain");

                var commands = domainAssembly.GetTypes()
                    .Where(t => t.IsNested && t.Name == "Handler")
                    .Select(t => t.GetInterfaces().First())
                    .Where(t => typeof(ICommandHandler<>).IsAssignableFrom(t.GetGenericTypeDefinition()))
                    .ToList();

                var events = domainAssembly.GetTypes()
                    .Where(t => t is IEvent)
                    .ToList();

                foreach (var commandType in commands)
                {
                    var typeArguments = commandType.GetGenericArguments();
                    x.AddConsumer(typeof(CommandConsumer<>).MakeGenericType(typeArguments));
                }

                x.AddBus(context => Bus.Factory.CreateUsingAzureServiceBus(cfg =>
                {
                    //cfg.ConfigureJsonSerializerOptions(config =>
                    //{
                    //    config.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                    //    return config;
                    //});
                    cfg.Host(credentials.ConnectionString, _ =>
                    {
                    });

                    cfg.ReceiveEndpoint("commands", ec =>
                    {
                        foreach (var commandType in commands)
                        {
                            var typeArguments = commandType.GetGenericArguments();
                            ec.ConfigureConsumer(context, typeof(CommandConsumer<>).MakeGenericType(typeArguments));
                        }
                    });

                    cfg.ReceiveEndpoint("events", ec =>
                    {
                        foreach (var eventType in events)
                        {
                            var typeArguments = eventType.GetGenericArguments();
                            ec.ConfigureConsumer(context, typeof(EventConsumer<>).MakeGenericType(typeArguments));
                        }
                    });
                }));
            });
        }

        public static void AddAzureServiceBusForAzureFunction(this IServiceCollection services, AzureServiceBusCredentials credentials, PostgresAzureCredentials postgresAzureCredentials)
        {
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddSingleton<IMessageReceiver, MessageReceiver>();
            services.AddSingleton<IAsyncBusHandle, AsyncBusHandle>();
            services.AddScoped<IIntegrationEventsQueue, IntegrationEventsQueue>();

            services.AddMassTransit(x =>
            {
                var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Single(assembly => assembly.GetName().Name == "BricksHoarder.Domain").GetTypes();

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

                foreach (var sagaType in sagas)
                {
                    x.AddSagaStateMachine(sagaType);
                }
                x.SetMartenSagaRepositoryProvider(postgresAzureCredentials.ConnectionString);

                var events = domainAssembly
                    .Where(t => t is IEvent)
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

                    //cfg.ReceiveEndpoint("commands", ec =>
                    //{
                    //    foreach (var commandType in commandsHandlersTypes)
                    //    {
                    //        var typeArguments = commandType.GetGenericArguments();
                    //        ec.ConfigureConsumer(context, typeof(CommandConsumer<>).MakeGenericType(typeArguments));
                    //    }
                    //});

                    cfg.UseServiceBusMessageScheduler();
                });
            });

            services.RemoveMassTransitHostedService();
        }
    }
}