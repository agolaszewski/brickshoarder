using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetPriceDecreased(string SetId, decimal Price) : IEvent;
}