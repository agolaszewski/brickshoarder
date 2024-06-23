using BricksHoarder.Core.Events;

namespace BricksHoarder.Events;

public record ThemeReleased(int ThemeId, int? ParentId, string Name) : IEvent;