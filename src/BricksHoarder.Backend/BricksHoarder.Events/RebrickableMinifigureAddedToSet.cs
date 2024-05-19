using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record RebrickableMinifigureAddedToSet(string SetId, int MinifigureId, string Name, int Quantity, string ImageUrl) : IEvent;
}