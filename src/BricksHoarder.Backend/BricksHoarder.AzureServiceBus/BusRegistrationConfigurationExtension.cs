using BricksHoarder.Common.DDD.Exceptions;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using BricksHoarder.Events;
using MassTransit;

namespace BricksHoarder.Azure.ServiceBus
{
    public static class BusRegistrationConfigurationExtension
    {
        public static void AddConsumerSaga<TStateMachine, T>(this IBusRegistrationConfigurator that, RedisCredentials redisCredentials) where TStateMachine : class, SagaStateMachine<T> where T : class, ISagaVersion, SagaStateMachineInstance
        {
            that.AddSagaStateMachine<TStateMachine, T>((context, config) =>
            {
                config.UseDelayedRedelivery(r =>
                {
                    r.Ignore<DomainException>();
                    r.Intervals(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(60));
                });

                config.UseMessageRetry(r =>
                {
                    r.Ignore<DomainException>();
                    r.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5));
                });

                config.UseInMemoryOutbox(context);
            }).RedisRepository(opt =>
            {
                opt.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                opt.DatabaseConfiguration(redisCredentials.ConnectionString);
            });
        }

        public static void AddConsumerBatch<TEvent>(this IBusRegistrationConfigurator that, Func<ConsumeContext<TEvent>, Guid?> correlationFn) where TEvent : class, IEvent
        {
            that.AddConsumer<BatchEventConsumer<TEvent>>(config =>
            {
                config.UseMessageRetry(r =>
                {
                    r.Ignore<DomainException>();
                    r.Intervals(TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3));
                });

                config.Options<BatchOptions>(options => options
                    .SetMessageLimit(1000)
                    .SetTimeLimit(s: 10)
                    .SetTimeLimitStart(BatchTimeLimitStart.FromFirst)
                    .GroupBy(correlationFn)
                    .SetConcurrencyLimit(10));
            }).Endpoint(config => { config.Name = $"brickshoarder.events/{typeof(TEvent).Name}"; });
        }

        public static void BatchSubscriptionEndpoint<TEvent>(this IServiceBusBusFactoryConfigurator that,
            IBusRegistrationContext context) where TEvent : class, IEvent
        {
            that.SubscriptionEndpoint("batch", $"brickshoarder.events/{typeof(TEvent).Name}", configureEndpoint =>
            {
                configureEndpoint.ConfigureConsumeTopology = false;

                configureEndpoint.LockDuration = TimeSpan.FromMinutes(5);
                configureEndpoint.PrefetchCount = 300;
                configureEndpoint.MaxConcurrentCalls = 10;
                configureEndpoint.MaxDeliveryCount = 5;

                configureEndpoint.UseInMemoryOutbox(context);

                configureEndpoint.ConfigureDeadLetterQueueDeadLetterTransport();
                configureEndpoint.ConfigureDeadLetterQueueErrorTransport();
                configureEndpoint.ConfigureConsumer<BatchEventConsumer<TEvent>>(context);
            });

            that.Message<BatchEvent<TEvent>>(x =>
            {
                x.SetEntityName($"brickshoarder.events/batch/{typeof(TEvent).Name}");
            });
        }

        public static void AddCommandConsumer<TCommand, TAggregateRoot>(this IBusRegistrationConfigurator that, Action<IConsumerConfigurator<CommandConsumer<TCommand, TAggregateRoot>>>? cfgFn = null)
            where TCommand : class, ICommand where TAggregateRoot : class, IAggregateRoot
        {
            that.AddConsumer<CommandConsumer<TCommand, TAggregateRoot>>(config =>
            {
                cfgFn?.Invoke(config);

                config.UseMessageRetry(r =>
                {
                    r.Ignore<DomainException>();
                    r.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3));
                });
            }).Endpoint(config => { config.Name = typeof(TCommand).Name; });
        }

        public static void AddScheduleCommandConsumer<TCommand, TEvent>(this IBusRegistrationConfigurator that) where TEvent : class, IEvent, IScheduling<TCommand> where TCommand : class, ICommand
        {
            that.AddConsumer<SchedulingConsumer<TCommand, TEvent>>(config =>
            {
                config.UseMessageRetry(r =>
                {
                    r.Ignore<DomainException>();
                    r.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(60));
                });
            });
        }

        public static void ScheduleSubscriptionEndpoint<TCommand, TEvent>(this IServiceBusBusFactoryConfigurator that,
            IBusRegistrationContext context) where TEvent : class, IEvent, IScheduling<TCommand> where TCommand : class, ICommand
        {
            that.SubscriptionEndpoint($"schedule.{typeof(TCommand).Name}", $"brickshoarder.events/{typeof(TEvent).Name}", configureEndpoint =>
            {
                configureEndpoint.ConfigureConsumeTopology = false;
                configureEndpoint.MaxDeliveryCount = 5;
                configureEndpoint.ConfigureDeadLetterQueueDeadLetterTransport();
                configureEndpoint.ConfigureDeadLetterQueueErrorTransport();
                configureEndpoint.ConfigureConsumer<SchedulingConsumer<TCommand, TEvent>>(context);
            });

            that.Publish<IScheduling<TCommand>>(x => x.Exclude = true);
        }

        public static void ConfigureCommandConsumer<TCommand, TAggregateRoot>(this IServiceBusBusFactoryConfigurator that, IBusRegistrationContext context) where TCommand : class, ICommand where TAggregateRoot : class, IAggregateRoot
        {
            that.ReceiveEndpoint(typeof(TCommand).Name, configureEndpoint =>
            {
                configureEndpoint.ConfigureConsumeTopology = false;
                configureEndpoint.MaxDeliveryCount = 3;
                configureEndpoint.ConfigureDeadLetterQueueDeadLetterTransport();
                configureEndpoint.ConfigureDeadLetterQueueErrorTransport();
                //configureEndpoint.UseInMemoryOutbox(context);

                configureEndpoint.ConfigureConsumer<CommandConsumer<TCommand, TAggregateRoot>>(context);
            });

            that.Message<CommandConsumed<TCommand>>(x =>
            {
                x.SetEntityName($"brickshoarder.events/consumed/{typeof(TCommand).Name}");
            });
        }
    }
}