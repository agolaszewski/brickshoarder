using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Credentials;
using MassTransit;

namespace BricksHoarder.Azure.ServiceBus
{
    public static class BusRegistrationConfigurationExtension
    {
        public static void AddConsumerSaga<TStateMachine, T>(this IBusRegistrationConfigurator that, RedisCredentials redisCredentials) where TStateMachine : class, SagaStateMachine<T> where T : class, ISagaVersion, SagaStateMachineInstance
        {
            that.AddSagaStateMachine<TStateMachine, T>((context, config) =>
            {
                config.UseInMemoryOutbox(context);
            }).RedisRepository(opt =>
            {
                opt.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                opt.DatabaseConfiguration(redisCredentials.ConnectionString);
            });
        }

        public static void AddCommandConsumer<TCommand, TAggregateRoot>(this IBusRegistrationConfigurator that) where TCommand : class, ICommand where TAggregateRoot : class, IAggregateRoot
        {
            that.AddConsumer<CommandConsumer<TCommand, TAggregateRoot>>().Endpoint(config =>
            {
                config.Name = typeof(TCommand).Name;
            });
        }
    }
}