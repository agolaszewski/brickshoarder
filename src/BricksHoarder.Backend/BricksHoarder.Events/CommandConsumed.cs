using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record CommandConsumed<TCommand>(TCommand Command, string CommandName) : IEvent;

    public class BatchEvent<TEvent> : IEvent
    {
        public Guid CorrelationId { get; set; }

        public IReadOnlyList<TEvent> Collection { get; set; }

        public BatchEvent()
        {
        }

        public BatchEvent(Guid correlationId, List<TEvent> messages)
        {
            Collection = messages;
            CorrelationId = correlationId;
        }
    }
}