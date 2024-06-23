using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Events;
using Rebrickable.Api;

namespace BricksHoarder.Domain.Set
{
    public class RebrickableSetAggregate : AggregateRoot<RebrickableSetAggregate>
    {
        internal void SetMinifigureData(LegoSetsMinifigsListAsyncResponse.Result minifigureApi)
        {
            AddEvent(new RebrickableMinifigureDataSynced(Id, minifigureApi.Id, minifigureApi.SetName, minifigureApi.Quantity, minifigureApi.SetImgUrl));
        }

        internal void SetData(LegoSetsReadAsyncResponse apiSet)
        {
            AddEvent(new RebrickableSetDataSynced(Id, apiSet.Name, apiSet.ThemeId, apiSet.Year, apiSet.NumParts, apiSet.SetImgUrl));
        }
    }
}