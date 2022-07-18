using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record SetCreated(string SetNumber, string Name, int Year, int ThemeId, int NumberOfParts, Uri? ImageUrl, DateTime LastModifiedDate) : IEvent
    {
        public Guid CorrelationId { get; init; }
    }
}