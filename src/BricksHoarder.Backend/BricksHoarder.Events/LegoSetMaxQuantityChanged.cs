using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetMaxQuantityChanged(string SetId, int? NewValue, int? OldValue) : IEvent;
}