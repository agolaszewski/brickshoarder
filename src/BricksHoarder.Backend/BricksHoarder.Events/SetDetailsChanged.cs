using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record SetDetailsChanged(string Id, DateTime LastModifiedDate) : IEvent;
}