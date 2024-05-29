using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Credentials;
using BricksHoarder.Events.Metadata;
using BricksHoarder.Events;
using MassTransit;
using BricksHoarder.Common.DDD.Exceptions;

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
                    r.Intervals(TimeSpan.FromMilliseconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5));
                });

                config.UseInMemoryOutbox(context);

            }).RedisRepository(opt =>
            {
                opt.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                opt.DatabaseConfiguration(redisCredentials.ConnectionString);
            });
        }

        public static void AddConsumerBatch<TEvent>(this IBusRegistrationConfigurator that) where TEvent : class, IEvent
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
                    .SetTimeLimit(s: 1)
                    .SetTimeLimitStart(BatchTimeLimitStart.FromLast)
                    .GroupBy<TEvent, Guid>(x => x.CorrelationId)
                    .SetConcurrencyLimit(10));

            }).Endpoint(config =>
            {
                config.Name = $"brickshoarder.events/{typeof(TEvent).Name}";
            });
        }

        public static void BatchSubscriptionEndpoint<TEvent>(this IServiceBusBusFactoryConfigurator that, IBusRegistrationContext context) where TEvent : class, IEvent
        {
            that.SubscriptionEndpoint("batch", $"brickshoarder.events/{typeof(TEvent).Name}", configureEndpoint =>
            {
                configureEndpoint.ConfigureConsumeTopology = false;
                configureEndpoint.MaxDeliveryCount = 5;
                configureEndpoint.ConfigureDeadLetterQueueDeadLetterTransport();
                configureEndpoint.ConfigureDeadLetterQueueErrorTransport();
                configureEndpoint.ConfigureConsumer<BatchEventConsumer<TEvent>>(context);
            });

            that.Message<BatchEvent<TEvent>>(x =>
            {
                x.SetEntityName($"brickshoarder.events/batch/{typeof(TEvent).Name}");
            });
        }

        public static void AddCommandConsumer<TCommand, TAggregateRoot>(this IBusRegistrationConfigurator that) where TCommand : class, ICommand where TAggregateRoot : class, IAggregateRoot
        {
            that.AddConsumer<CommandConsumer<TCommand, TAggregateRoot>>(config =>
            {


            }).Endpoint(config =>
            {
                config.Name = typeof(TCommand).Name;
            });
        }
    }
}