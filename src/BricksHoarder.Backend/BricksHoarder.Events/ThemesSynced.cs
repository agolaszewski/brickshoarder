using BricksHoarder.Core.Events;
using MassTransit;

namespace BricksHoarder.Events
{
    public record ThemesSynced : CorrelatedBy<Guid>, IEvent
    {
        public Guid CorrelationId { get; set; }
    }
}