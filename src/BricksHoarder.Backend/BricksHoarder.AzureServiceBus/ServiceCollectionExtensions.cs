using BricksHoarder.Azure.ServiceBus.Services;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using BricksHoarder.Domain.LegoSet;
using BricksHoarder.Domain.RebrickableSet;
using BricksHoarder.Domain.SetsCollection;
using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Domain.ThemesCollection;
using BricksHoarder.Events;
using BricksHoarder.Events.Metadata;
using MassTransit;
using MassTransit.AzureServiceBusTransport;
using MassTransit.Configuration;
using MassTransit.Scheduling;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BricksHoarder.Azure.ServiceBus
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAzureServiceBus2(this IServiceCollection services, AzureServiceBusCredentials credentials, RedisCredentials redisCredentials)
        {
            services.AddScoped<RequestToCommandMapper>();
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<IEventDispatcher, EventDispatcher>();
            services.AddScoped<IIntegrationEventsQueue, IntegrationEventsQueue>();

            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(credentials.ConnectionString).WithName("ServiceBusClient");
                builder.AddServiceBusAdministrationClient(credentials.ConnectionString)
                    .WithName("ServiceBusAdministrationClient");
            });
            services.AddScoped<DeadLetterQueueRescheduler>();
            services.AddScoped<ResubmitDeadQueueService>();

            services.AddMassTransit(x =>
            {
                x.AddCommandConsumer<SyncThemesCommand, ThemesCollectionAggregate>();
                x.AddCommandConsumer<SyncSetsCommand, SetsCollectionAggregate>();
                x.AddCommandConsumer<SyncSetRebrickableDataCommand, RebrickableSetAggregate>();
                x.AddCommandConsumer<SyncSetLegoDataCommand, LegoSetAggregate>();

                var domainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Single(assembly => assembly.GetName().Name == "BricksHoarder.Domain").GetTypes();

                var eventsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Single(assembly => assembly.GetName().Name == "BricksHoarder.Events").GetTypes();

                var commandsHandlersTypes = domainAssembly
                    .Where(t => t.IsNested && t.Name == "Handler")
                    .Select(t => t.GetInterfaces().First())
                    .Where(t => typeof(ICommandHandler<,>).IsAssignableFrom(t.GetGenericTypeDefinition()))
                    .ToList();

                //foreach (var commandHandlerType in commandsHandlersTypes)
                //{
                //    var typeArguments = commandHandlerType.GetGenericArguments();
                //    x.AddConsumer(typeof(CommandConsumer<,>).MakeGenericType(typeArguments));
                //}
                
                #region Sagas Consumers

                x.AddConsumerSaga<SyncRebrickableDataSaga, SyncRebrickableDataSagaState>(redisCredentials);

                x.AddConsumer<BatchEventConsumer<SetReleased>>(config =>
                {
                    config.Options<BatchOptions>(options => options
                        .SetMessageLimit(100)
                        .SetTimeLimit(s: 1)
                        .SetTimeLimitStart(BatchTimeLimitStart.FromLast)
                        .GroupBy<SetReleased, Guid>(x => x.CorrelationId)
                        .SetConcurrencyLimit(10));

                }).Endpoint(config =>
                {
                    config.Name = nameof(SetReleasedBatchMetadata.TopicPath);
                });

                x.AddConsumer<BatchEventConsumer<SetDetailsChanged>>(config =>
                {
                    config.Options<BatchOptions>(options => options
                        .SetMessageLimit(100)
                        .SetTimeLimit(s: 1)
                        .SetTimeLimitStart(BatchTimeLimitStart.FromLast)
                        .GroupBy<SetDetailsChanged, Guid>(x => x.CorrelationId)
                        .SetConcurrencyLimit(10));

                }).Endpoint(config =>
                {
                    config.Name = SetDetailsChangedMetadata.TopicPath;
                });

                #endregion Sagas Consumers

                //x.AddConsumer(typeof(SchedulingConsumer<SyncSetLegoDataCommand, LegoSetInSale>));
                //x.AddConsumer(typeof(SchedulingConsumer<SyncSetLegoDataCommand, LegoSetToBeReleased>));
                //x.AddConsumer(typeof(SchedulingConsumer<SyncSetLegoDataCommand, LegoSetPending>));

                x.AddServiceBusMessageScheduler();

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    var options = context.GetRequiredService<IOptions<ServiceBusOptions>>();
                    cfg.Host(credentials.ConnectionString, _ => { });

                    foreach (var commandHandlerType in commandsHandlersTypes)
                    {
                        var typeArguments = commandHandlerType.GetGenericArguments();
                        var command = typeArguments[0];

                        cfg.ReceiveEndpoint(command.Name, configureEndpoint =>
                        {
                            configureEndpoint.ConfigureConsumeTopology = false;
                            configureEndpoint.MaxDeliveryCount = 1;
                            configureEndpoint.ConfigureDeadLetterQueueDeadLetterTransport();
                            configureEndpoint.ConfigureDeadLetterQueueErrorTransport();

                            var consumerType = typeof(CommandConsumer<,>).MakeGenericType(typeArguments);
                            configureEndpoint.ConfigureConsumer(context, consumerType);
                        });
                    }

                    cfg.SubscriptionEndpoint("default", SetReleasedMetadata.TopicPath, configureEndpoint =>
                    {
                        configureEndpoint.ConfigureConsumeTopology = false;
                        configureEndpoint.MaxDeliveryCount = 1;
                        configureEndpoint.ConfigureDeadLetterQueueDeadLetterTransport();
                        configureEndpoint.ConfigureDeadLetterQueueErrorTransport();
                        configureEndpoint.ConfigureConsumer<BatchEventConsumer<SetReleased>>(context);
                    });

                    cfg.SubscriptionEndpoint("default", SetDetailsChangedMetadata.TopicPath, configureEndpoint =>
                    {
                        configureEndpoint.ConfigureConsumeTopology = false;
                        configureEndpoint.MaxDeliveryCount = 1;
                        configureEndpoint.ConfigureDeadLetterQueueDeadLetterTransport();
                        configureEndpoint.ConfigureDeadLetterQueueErrorTransport();
                        configureEndpoint.ConfigureConsumer<BatchEventConsumer<SetDetailsChanged>>(context);
                    });

                    cfg.Message<BatchEvent<SetReleased>>(x =>
                    {
                        x.SetEntityName(SetReleasedBatchMetadata.TopicPath);
                    });

                    cfg.Publish<IEvent>(x => x.Exclude = true);
                    cfg.Publish<ICommand>(x => x.Exclude = true);

                    cfg.UseMessageRetry(r => r.Intervals(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5)));

                    cfg.ConfigureEndpoints(context);
                });
            });
        }

        public static void AddAzureServiceBus(this IServiceCollection services, AzureServiceBusCredentials credentials, RedisCredentials redisCredentials)
        {
            services.AddScoped<RequestToCommandMapper>();
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<IEventDispatcher, EventDispatcher>();
            services.AddScoped<IIntegrationEventsQueue, IntegrationEventsQueue>();

            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(credentials.ConnectionString).WithName("ServiceBusClient");
                builder.AddServiceBusAdministrationClient(credentials.ConnectionString).WithName("ServiceBusAdministrationClient");
            });
            services.AddScoped<DeadLetterQueueRescheduler>();
            services.AddScoped<ResubmitDeadQueueService>();

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

                x.AddServiceBusMessageScheduler();

                x.AddSagaStateMachine<SyncRebrickableDataSaga, SyncRebrickableDataSagaState>((context, config) =>
                {
                    config.UseInMemoryOutbox(context);
                }).RedisRepository(opt =>
                {
                    opt.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    opt.DatabaseConfiguration(redisCredentials.ConnectionString);
                });

                var events = eventsAssembly
                    .Where(t => t.GetInterface(nameof(IEvent)) is not null)
                    .ToList();

                foreach (var eventType in events)
                {
                    //x.AddConsumer(typeof(EventConsumer<>).MakeGenericType(eventType));
                }

                //SchedulingConsumers
                x.AddConsumer(typeof(SchedulingConsumer<SyncSetLegoDataCommand, LegoSetInSale>));
                x.AddConsumer(typeof(SchedulingConsumer<SyncSetLegoDataCommand, LegoSetToBeReleased>));
                x.AddConsumer(typeof(SchedulingConsumer<SyncSetLegoDataCommand, LegoSetPending>));

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    var options = context.GetRequiredService<IOptions<ServiceBusOptions>>();
                    //options.Value.AutoCompleteMessages = true;

                    cfg.Host(credentials.ConnectionString, _ =>
                    {
                    });

                    cfg.ReceiveEndpoint("SyncSetLegoDataCommand", configureEndpoint =>
                    {
                        configureEndpoint.ConfigureConsumeTopology = false;
                        configureEndpoint.MaxDeliveryCount = 1;
                        configureEndpoint.ForwardDeadLetteredMessagesTo = "brickshoarder/fault";
                    });

                    cfg.Publish<IEvent>(x => x.Exclude = true);
                    cfg.Publish<ICommand>(x => x.Exclude = true);

                    //cfg.Message<CommandConsumed<SyncThemesCommand>>(x =>
                    //{
                    //    x.SetEntityName(SyncThemesCommandConsumedMetadata.TopicPath);
                    //});

                    //cfg.Message<CommandConsumed<SyncSetsCommand>>(x =>
                    //{
                    //    x.SetEntityName(SyncSetsCommandConsumedMetadata.TopicPath);
                    //});

                    //cfg.Message<CommandConsumed<SyncSetRebrickableDataCommand>>(x =>
                    //{
                    //    x.SetEntityName(SyncSetRebrickableDataCommandConsumedMetadata.TopicPath);
                    //});

                    //cfg.Message<Batch<SetReleased>>(x =>
                    //{
                    //    x.SetEntityName(SetReleasedBatchMetadata.TopicPath);
                    //});

                    //cfg.Message<Batch<SetDetailsChanged>>(x =>
                    //{
                    //    x.SetEntityName(SetDetailsChangedBatchMetadata.TopicPath);
                    //});

                    cfg.Message<Batch<SetReleased>>(x =>
                    {
                        x.SetEntityName(SetReleasedBatchMetadata.TopicPath);
                    });

                    cfg.UseServiceBusMessageScheduler();

                    cfg.UseMessageRetry(r => r.Intervals(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5)));
                    cfg.AutoStart = true;
                });
            });
        }

        public static void AddAzureServiceBusForAzureFunction(this IServiceCollection services, AzureServiceBusCredentials credentials, RedisCredentials redisCredentials)
        {
            services.AddScoped<RequestToCommandMapper>();
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
            services.AddScoped<ResubmitDeadQueueService>();

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

                x.AddServiceBusMessageScheduler();

                var sagas = domainAssembly.Where(t => t.Name.EndsWith("Saga"));

                x.AddSagaStateMachine<SyncRebrickableDataSaga, SyncRebrickableDataSagaState>((context, config) =>
                {
                    config.UseInMemoryOutbox(context);
                }).RedisRepository(opt =>
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

                //SchedulingConsumers
                x.AddConsumer(typeof(SchedulingConsumer<SyncSetLegoDataCommand, LegoSetInSale>));
                x.AddConsumer(typeof(SchedulingConsumer<SyncSetLegoDataCommand, LegoSetToBeReleased>));
                x.AddConsumer(typeof(SchedulingConsumer<SyncSetLegoDataCommand, LegoSetPending>));

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

                    cfg.Message<CommandConsumed<SyncSetRebrickableDataCommand>>(x =>
                    {
                        x.SetEntityName(SyncSetRebrickableDataCommandConsumedMetadata.TopicPath);
                    });

                    cfg.Message<Batch<SetReleased>>(x =>
                    {
                        x.SetEntityName(SetReleasedBatchMetadata.TopicPath);
                    });

                    cfg.Message<Batch<SetDetailsChanged>>(x =>
                    {
                        x.SetEntityName(SetDetailsChangedBatchMetadata.TopicPath);
                    });

                    cfg.UseServiceBusMessageScheduler();

                    cfg.UseMessageRetry(r => r.Intervals(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5)));

                    cfg.ConfigureEndpoints(context);
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