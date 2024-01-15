using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record RebrickableSetDataSynced(string SetId, string Name, int ThemeId, int Year, int NumberOfParts, string ImageUrl) : IEvent;
}