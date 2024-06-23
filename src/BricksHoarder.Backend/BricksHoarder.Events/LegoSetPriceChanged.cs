using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetPriceChanged(string SetId, decimal? NewValue, decimal? OldValue) : IEvent;
}