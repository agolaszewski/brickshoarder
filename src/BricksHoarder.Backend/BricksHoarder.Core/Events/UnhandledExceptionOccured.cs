namespace BricksHoarder.Core.Events
{
    public class UnhandledExceptionOccured : IEvent
    {
        public Guid Id { get; set; }

        public string CommandFullName { get; set; }
        public Guid CommandCorrelationId { get; set; }
        public Guid CorrelationId { get; set; }
        public string Message { get; set; }
    }
}
