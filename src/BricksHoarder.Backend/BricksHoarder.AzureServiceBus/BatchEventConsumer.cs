using BricksHoarder.Core.Events;
using BricksHoarder.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Azure.ServiceBus
{
    public class BatchEventConsumer<TEvent> : IConsumer<Batch<TEvent>> where TEvent : class, IEvent
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ILogger<BatchEventConsumer<TEvent>> _logger;

        public BatchEventConsumer(IEventDispatcher eventDispatcher, ILogger<BatchEventConsumer<TEvent>> logger)
        {
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<Batch<TEvent>> context)
        {
            try
            {
                _logger.LogInformation("Consuming batch {Event} {Number} {CorrelationId}", typeof(TEvent).Name, context.Message.Length, context.CorrelationId);

                BatchEvent<TEvent> batchEvent = new BatchEvent<TEvent>(context.CorrelationId!.Value, context.Message.Select(m => m.Message).ToList());
                await _eventDispatcher.DispatchAsync(batchEvent, context.CorrelationId!.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception In BatchEventConsumer {Message} {CorrelationId} {Content}", context.Message.GetType().FullName, context.CorrelationId, System.Text.Json.JsonSerializer.Serialize(context.Message));
                throw;
            }
            finally
            {
                _logger.LogDebug("Consumed {Message} {CorrelationId}", context.Message.GetType().FullName, context.CorrelationId);
            }
        }
    }
}