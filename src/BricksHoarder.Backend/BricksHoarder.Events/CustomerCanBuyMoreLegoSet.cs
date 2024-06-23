using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record CustomerCanBuyMoreLegoSet(string SetId, decimal Price) : IEvent;
}