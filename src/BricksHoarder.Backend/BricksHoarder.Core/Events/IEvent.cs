namespace BricksHoarder.Core.Events
{
    public interface IEvent
    {
        Guid CorrelationId { get; init; }
    }
}
