using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetIsAvailable(string SetId, DateTime AvailableSince) : IEvent;
}