using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record ThemesSynced : IEvent
    {
        public Guid HerpDerp { get; set; } = Guid.NewGuid();
    }
}