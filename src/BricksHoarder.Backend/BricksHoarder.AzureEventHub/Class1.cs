using BricksHoarder.Core.Events;

namespace BricksHoarder.AzureEventHub
{
    public class IntegrationEventsDispatcher : IIntegrationEventDispatcher
    {
        private IIntegrationEventsQueue _queue;

        public IntegrationEventsDispatcher(IIntegrationEventsQueue queue)
        {
            _queue = queue;
        }


        public Task DispatchAsync()
        {
            throw new NotImplementedException();
        }

        public void Queue<TEvent>(TEvent @event) where TEvent : class, IEvent
        {
            throw new NotImplementedException();
        }
    }
}