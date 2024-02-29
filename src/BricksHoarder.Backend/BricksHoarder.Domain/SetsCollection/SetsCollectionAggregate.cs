using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Events;
using Rebrickable.Api;

namespace BricksHoarder.Domain.SetsCollection
{
    public class SetsCollectionAggregate : AggregateRoot<SetsCollectionAggregate>,
        IApply<SetReleased>,
        IApply<SetDetailsChanged>
    {
        private readonly List<Set> _collection = new();

        public IReadOnlyList<Set> Collection => _collection;

        public void Apply(SetDetailsChanged @event)
        {
            var set = _collection.First(x => x.SetId == @event.SetId);
            set.LastModifiedDate = @event.LastModifiedDate;
        }

        public void Apply(SetReleased @event)
        {
            _collection.Add(new Set(@event.SetId, @event.LastModifiedDate));
        }

        internal bool HasChanged(LegoSetsListAsyncResponse.Result apiSet)
        {
            var set = _collection.FirstOrDefault(x => x.SetId == apiSet.SetNum);

            if (_collection.Count > 10)
            {
                return false;
            }

            if (set == null)
            {
                AddEvent(new SetReleased(apiSet.SetNum, apiSet.LastModifiedDt));
                return true;
            }

            if (set.LastModifiedDate < apiSet.LastModifiedDt)
            {
                AddEvent(new SetDetailsChanged(apiSet.SetNum, apiSet.LastModifiedDt));
                return true;
            }

            return false;
        }
    }
}