using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record RebrickableMinifigureDataFetched(string SetId, string MinifigureId, string MinifigureName, int Quantity,
        string ImageUrl) : IEvent;
}