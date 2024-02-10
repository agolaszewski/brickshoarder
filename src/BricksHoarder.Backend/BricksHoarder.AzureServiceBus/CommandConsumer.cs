using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.AzureCloud.ServiceBus
{
    public class CommandConsumer<TCommand, TAggregateRoot> : IConsumer<TCommand> where TCommand : class, ICommand where TAggregateRoot : class, IAggregateRoot
    {
        private readonly ICommandHandler<TCommand, TAggregateRoot> _handler;
        private readonly IAggregateStore _aggregateStore;
        private readonly ILogger<CommandConsumer<TCommand, TAggregateRoot>> _logger;
        private readonly IIntegrationEventsQueue _integrationEventsQueue;

        public CommandConsumer(
            ICommandHandler<TCommand, TAggregateRoot> handler,
            IAggregateStore aggregateStore,
            IIntegrationEventsQueue integrationEventsQueue,
            ILogger<CommandConsumer<TCommand, TAggregateRoot>> logger)
        {
            _handler = handler;
            _aggregateStore = aggregateStore;
            _integrationEventsQueue = integrationEventsQueue;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TCommand> context)
        {
            try
            {
                _logger.LogDebug($"Consuming {context.Message.GetType().FullName} {context.CorrelationId}");

                TAggregateRoot aggregateRoot = await _handler.HandleAsync(context.Message);
                await _aggregateStore.SaveAsync(aggregateRoot);

                var tasks = new List<Task>();
                foreach (var @event in aggregateRoot.Events)
                {
                    tasks.Add(context.Publish(@event.Event, @event.Event.GetType(), x => x.CorrelationId = context.CorrelationId));
                }
                await Task.WhenAll(tasks);

                await context.Publish(new CommandConsumed<TCommand>(context.Message, typeof(TCommand).FullName!), x => x.CorrelationId = context.CorrelationId);

                foreach (var @event in _integrationEventsQueue.Events)
                {
                    await context.Publish(@event, @event.GetType(), x => x.CorrelationId = context.CorrelationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,$"Exception {context.Message.GetType().FullName} {context.CorrelationId!}");
                throw;
            }
            finally
            {
                _logger.LogDebug($"Consumed {context.Message.GetType().FullName} {context.CorrelationId!}");
            }
        }
    }
}