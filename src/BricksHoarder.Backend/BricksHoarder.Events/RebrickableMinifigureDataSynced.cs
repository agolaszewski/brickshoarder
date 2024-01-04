using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record RebrickableMinifigureDataSynced(string SetId, string MinifigureId, string MinifigureName, int Quantity, string ImageUrl) : IEvent;
}