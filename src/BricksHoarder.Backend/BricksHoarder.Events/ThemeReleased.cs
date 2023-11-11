using BricksHoarder.Core.Events;

namespace BricksHoarder.Events;

public record ThemeReleased(int Id, int? ParentId, string Name) : IEvent;