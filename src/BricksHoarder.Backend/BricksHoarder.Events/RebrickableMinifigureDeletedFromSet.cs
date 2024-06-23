using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record RebrickableMinifigureDeletedFromSet(int MinifigureId, string SetId) : IEvent;
}