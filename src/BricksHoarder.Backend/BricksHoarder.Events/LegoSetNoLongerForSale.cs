using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetNoLongerForSale(string SetId, DateTime DiscontinuedSince) : IEvent;
}