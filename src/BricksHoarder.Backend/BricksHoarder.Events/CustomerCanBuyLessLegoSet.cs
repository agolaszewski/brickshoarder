using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record CustomerCanBuyLessLegoSet(string SetId, decimal Price) : IEvent;
}