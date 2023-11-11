using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record SetReleased(string Id, DateTime LastModifiedDate) : IEvent;
}