using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record SetReleased(string SetId, DateTime LastModifiedDate) : IEvent;
}