using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetPriceIncreased(string SetId, decimal Price) : IEvent;
}