using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events;
using BricksHoarder.Events.Metadata;
using BricksHoarder.MassTransit;
using MassTransit;
using MassTransit.AzureServiceBusTransport;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BricksHoarder.AzureCloud.ServiceBus
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAzureServiceBusForAzureFunction(this IServiceCollection services, AzureServiceBusCredentials credentials, SqlServerDatabaseCredentials sqlServerDatabaseCredentials)
        {
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<IEventDispatcher, EventDispatcher>();
            services.AddSingleton<IMessageReceiver, MessageReceiver>();
            services.AddSingleton<IAsyncBusHandle, AsyncBusHandle>();
            services.AddScoped<IIntegrationEventsQueue, IntegrationEventsQueue>();
            services.AddOutbox(sqlServerDatabaseCredentials);

            services.AddMassTransit(x =>
            {
                var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Single(assembly => assembly.GetName().Name == "BricksHoarder.Domain").GetTypes();

                var eventsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Single(assembly => assembly.GetName().Name == "BricksHoarder.Events").GetTypes();

                var commandsHandlersTypes = domainAssembly
                    .Where(t => t.IsNested && t.Name == "Handler")
                    .Select(t => t.GetInterfaces().First())
                    .Where(t => typeof(ICommandHandler<,>).IsAssignableFrom(t.GetGenericTypeDefinition()))
                    .ToList();

                foreach (var commandHandlerType in commandsHandlersTypes)
                {
                    var typeArguments = commandHandlerType.GetGenericArguments();
                    x.AddConsumer(typeof(CommandConsumer<,>).MakeGenericType(typeArguments));
                }

                var sagas = domainAssembly
                    .Where(t => t.Name.EndsWith("Saga"));

                x.AddSagaStateMachine<SyncRebrickableDataSaga, SyncRebrickableDataSagaState>().InMemoryRepository();
                //.EntityFrameworkRepository(r =>
                //{
                //    r.ExistingDbContext<MassTransitDbContext>();
                //    r.UseSqlServer();
                //});

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
                        x.SetEntityName(SyncThemesCommandConsumedMetadata.TopicPath);
                    });

                    cfg.Message<CommandConsumed<SyncSetsCommand>>(x =>
                    {
                        x.SetEntityName(SyncSetsCommandConsumedMetadata.TopicPath);
                    });

                    cfg.Message<CommandConsumed<FetchSetRebrickableDataCommand>>(x =>
                    {
                        x.SetEntityName(FetchSetRebrickableDataCommandConsumedMetadata.TopicPath);
                    });

                    cfg.UseServiceBusMessageScheduler();

                    cfg.UseMessageRetry(r => r.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)));
                });

                //x.AddEntityFrameworkOutbox<MassTransitDbContext>(o =>
                //{
                //    o.UseSqlServer();
                //    o.UseBusOutbox();
                //});
            });

            services.RemoveMassTransitHostedService();
        }
    }
}