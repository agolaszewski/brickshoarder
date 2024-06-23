using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record RebrickableMinifigureDataSynced(string SetId, int MinifigureId, string Name, int Quantity, string ImageUrl) : IEvent;
}