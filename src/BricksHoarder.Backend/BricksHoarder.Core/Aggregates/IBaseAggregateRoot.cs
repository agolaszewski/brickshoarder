using BricksHoarder.Core.Events;

namespace BricksHoarder.Core.Aggregates
{
    public interface IBaseAggregateRoot
    {
        IEnumerable<EventComposite> Events { get; }

        Guid Id { get; set; }

        long Version { get; set; }

        public bool IsDeleted { get; set; }
    }
}
