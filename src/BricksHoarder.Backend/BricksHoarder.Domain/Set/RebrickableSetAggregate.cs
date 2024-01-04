using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Events;
using Rebrickable.Api;

namespace BricksHoarder.Domain.Set
{
    public class RebrickableSetAggregate : AggregateRoot<RebrickableSetAggregate>
    {
        internal void SetValuesOfMinifigure(LegoSetsMinifigsListAsyncResponse.Result minifigureApi)
        {
            AddEvent(new RebrickableMinifigureDataFetched(Id, minifigureApi.SetNum, minifigureApi.SetName, minifigureApi.Quantity, minifigureApi.SetImgUrl));
        }

        internal void SetValues(LegoSetsReadAsyncResponse apiSet)
        {
            AddEvent(new RebrickableSetDataFetched(Id, apiSet.Name, apiSet.ThemeId, apiSet.Year, apiSet.NumParts, apiSet.SetImgUrl));
        }
    }
}