using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetUpdated(string SetId, LegoSetAvailability Availability, int? MaxQuantity, decimal? Price) : IEvent;
}