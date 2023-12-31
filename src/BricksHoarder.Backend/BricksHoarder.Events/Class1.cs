using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record RebrickableDataFetched(string Id, string Name, int ThemeId, int Year, int NumberOfParts, string ImageUrl) : IEvent;
}