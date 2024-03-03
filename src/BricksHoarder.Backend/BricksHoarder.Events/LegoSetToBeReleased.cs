using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetToBeReleased(string SetId, DateTime ReleaseDate) : IEvent;

    public record LegoSetNotChanged(string SetId) : IEvent;
}