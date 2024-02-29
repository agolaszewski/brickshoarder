using BricksHoarder.Azure.ServiceBus.Services;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events;
using BricksHoarder.Events.Metadata;
using MassTransit;
using MassTransit.AzureServiceBusTransport;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BricksHoarder.AzureCloud.ServiceBus
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAzureServiceBusForAzureFunction(this IServiceCollection services, AzureServiceBusCredentials credentials, RedisCredentials redisCredentials)
        {
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<IEventDispatcher, EventDispatcher>();
            services.AddSingleton<IMessageReceiver, MessageReceiver>();
            services.AddSingleton<IAsyncBusHandle, AsyncBusHandle>();
            services.AddScoped<IIntegrationEventsQueue, IntegrationEventsQueue>();
            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(credentials.ConnectionString).WithName("ServiceBusClient");
                builder.AddServiceBusAdministrationClient(credentials.ConnectionString).WithName("ServiceBusAdministrationClient");
            });
            services.AddScoped<DeadLetterQueueRescheduler>();

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

                var sagas = domainAssembly.Where(t => t.Name.EndsWith("Saga"));

                x.AddSagaStateMachine<SyncRebrickableDataSaga, SyncRebrickableDataSagaState>().RedisRepository(opt =>
                {
                    opt.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    opt.DatabaseConfiguration(redisCredentials.ConnectionString);
                });

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
                    cfg.Publish<IBatch>(x => x.Exclude = true);
                    cfg.Publish<ICommand>(x => x.Exclude = true);

                    cfg.Message<CommandConsumed<SyncThemesCommand>>(x =>
                    {
                        x.SetEntityName(SyncThemesCommandConsumedMetadata.TopicPath);
                    });

                    cfg.Message<CommandConsumed<SyncSetsCommand>>(x =>
                    {
                        x.SetEntityName(SyncSetsCommandConsumedMetadata.TopicPath);
                    });

                    cfg.Message<CommandConsumed<SyncSetRebrickableDataCommand>>(x =>
                    {
                        x.SetEntityName(SyncSetRebrickableDataCommandConsumedMetadata.TopicPath);
                    });

                    cfg.Message<Fault<SyncThemesCommand>>(x =>
                    {
                        x.SetEntityName(SyncThemesCommandFaultedMetadata.TopicPath);
                    });

                    cfg.Message<BatchEvent<SetReleased>>(x =>
                    {
                        x.SetEntityName(SetReleasedBatchMetadata.TopicPath);
                    });

                    cfg.UseServiceBusMessageScheduler();

                    cfg.UseMessageRetry(r => r.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5)));
                });

                //x.AddEntityFrameworkOutbox<MassTransitDbContext>(o =>
                //{
                //    o.UseSqlServer();
                //    o.UseBusOutbox();
                //});

                //x.AddConfigureEndpointsCallback((context, name, cfg) =>
                //{
                //    cfg.UseEntityFrameworkOutbox<MassTransitDbContext>(context);
                //});
            });

            //services.RemoveMassTransitHostedService();
        }
    }
}