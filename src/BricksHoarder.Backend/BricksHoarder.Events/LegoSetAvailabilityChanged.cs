using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetAvailabilityChanged(string SetId, LegoSetAvailability NewValue, LegoSetAvailability OldValue) : IEvent;
}