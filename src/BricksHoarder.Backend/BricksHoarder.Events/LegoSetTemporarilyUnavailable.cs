using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetTemporarilyUnavailable(string SetId, DateTime TemporarilyUnavailableSince) : IEvent;
}