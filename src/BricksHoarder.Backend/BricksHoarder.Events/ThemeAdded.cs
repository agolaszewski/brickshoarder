using BricksHoarder.Core.Events;

namespace BricksHoarder.Events;

public record ThemeAdded(int Id, int? ParentId, string Name) : IEvent;