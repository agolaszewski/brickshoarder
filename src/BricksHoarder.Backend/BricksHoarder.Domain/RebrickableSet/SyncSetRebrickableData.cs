using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using Rebrickable.Api;

namespace BricksHoarder.Domain.RebrickableSet
{
    public class SyncSetRebrickableData
    {
        public class Handler(IRebrickableClient rebrickableClient, IAggregateStore aggregateStore) : ICommandHandler<SyncSetRebrickableDataCommand, RebrickableSetAggregate>
        {
            public async Task<RebrickableSetAggregate> HandleAsync(SyncSetRebrickableDataCommand command)
            {
                var set = await aggregateStore.GetByIdOrDefaultAsync<RebrickableSetAggregate>(command.SetId);

                var apiSet = await rebrickableClient.LegoSetsReadAsync(command.SetId);
                set.SetData(apiSet);

                var minifigures = await rebrickableClient.LegoSetsMinifigsListAsync(command.SetId);
                set.SetMinifigureData(minifigures.Results);

                return set;
            }
        }
    }
}