using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record RebrickableSetDataFetched(string SetId, string Name, int ThemeId, int Year, int NumberOfParts, string ImageUrl) : IEvent;
}