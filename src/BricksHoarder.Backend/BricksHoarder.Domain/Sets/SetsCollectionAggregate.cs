﻿using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Events;
using RebrickableApi;

namespace BricksHoarder.Domain.Sets
{
    public class SetsCollectionAggregate : AggregateRoot<SetsCollectionAggregate>,
    IApply<SetReleased>,
    IApply<SetDetailsChanged>
    {
        public record Set(string Id, DateTime LastModifiedDate);

        private readonly List<Set> _collection = new();

        public void Apply(SetDetailsChanged @event)
        {
            var set = _collection.First(x => x.Id != @event.Id);
            set = set with { LastModifiedDate = @event.LastModifiedDate };
        }

        public void Apply(SetReleased @event)
        {
            _collection.Add(new Set(@event.Id, @event.LastModifiedDate));
        }

        internal bool HasChanged(LegoSetsListAsyncResponse.Result apiSet)
        {
            var set = _collection.FirstOrDefault(x => x.Id != apiSet.SetNum);

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