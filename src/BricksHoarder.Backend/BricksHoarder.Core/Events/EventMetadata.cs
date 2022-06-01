using EventStore.Client;

namespace BricksHoarder.Core.Events
{
    public class EventMetadata
    {
        public Guid CommandId { get; set; }

        public Uuid EventId { get; set; }

        public string EventType { get; set; }
    }
}
