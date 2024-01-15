using Azure.Messaging.EventGrid;
using BricksHoarder.Core.Events;
using MassTransit;

namespace BricksHoarder.AzureEventGrid
{
    public class IntegrationEventsDispatcher : IIntegrationEventDispatcher
    {
        private IIntegrationEventsQueue _queue;
        private readonly EventGridPublisherClient _eventGridPublisherClient;

        public IntegrationEventsDispatcher(IIntegrationEventsQueue queue, EventGridPublisherClient eventGridPublisherClient)
        {
            _queue = queue;
            _eventGridPublisherClient = eventGridPublisherClient;
        }

        public async Task DispatchAsync()
        {
            var events = ToEventGridEvent(_queue.Events);
            await _eventGridPublisherClient.SendEventsAsync(events);
        }

        private IReadOnlyList<EventGridEvent> ToEventGridEvent(IReadOnlyCollection<IEvent> events)
        {
            return events.Select(e => new EventGridEvent("integration-events", e.GetType().FullName, "1", new BinaryData(e, null, e.GetType()))).ToList();
        }

        public Task DispatchAsync(ConsumeContext context)
        {
            throw new NotImplementedException();
        }
    }
}