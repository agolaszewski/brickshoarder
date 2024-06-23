using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record SetDetailsChanged(string SetId, DateTime LastModifiedDate) : IEvent;
}