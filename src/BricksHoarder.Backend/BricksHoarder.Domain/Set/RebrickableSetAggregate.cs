using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Events;
using Rebrickable.Api;

namespace BricksHoarder.Domain.Set
{
    public class RebrickableSetAggregate : AggregateRoot<RebrickableSetAggregate>
    {
        internal void SetValues(LegoSetsReadAsyncResponse apiSet)
        {
            AddEvent(new RebrickableDataFetched(apiSet.SetNum, apiSet.Name, apiSet.ThemeId, apiSet.Year, apiSet.NumParts, apiSet.SetImgUrl));
        }
    }
}