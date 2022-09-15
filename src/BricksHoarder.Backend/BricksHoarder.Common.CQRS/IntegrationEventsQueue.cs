using BricksHoarder.Core.Events;

namespace BricksHoarder.Common.CQRS
{
    public class IntegrationEventsQueue : IIntegrationEventsQueue
    {
        private Queue<IEvent> _queue = new Queue<IEvent>();

        public IReadOnlyCollection<IEvent> Events => _queue;

        public void Queue<TEvent>(TEvent @event) where TEvent : class, IEvent
        {
            _queue.Enqueue(@event);
        }
    }
}