using BricksHoarder.Core.Events;
using MassTransit;

namespace BricksHoarder.RabbitMq
{
    public class IntegrationEventsDispatcher : IIntegrationEventDispatcher
    {
        private readonly IIntegrationEventsQueue _queue;

        public IntegrationEventsDispatcher(IIntegrationEventsQueue queue)
        {
            _queue = queue;
        }

        public async Task DispatchAsync(ConsumeContext context)
        {
            await context.PublishBatch(_queue.Events);
        }
    }
}