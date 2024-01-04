using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using Rebrickable.Api;

namespace BricksHoarder.Domain.Set
{
    public class SyncSetRebrickableData
    {
        public class Handler : ICommandHandler<SyncSetRebrickableDataCommand, RebrickableSetAggregate>
        {
            private readonly IRebrickableClient _rebrickableClient;
            private readonly IAggregateStore _aggregateStore;

            public Handler(IRebrickableClient rebrickableClient, IAggregateStore aggregateStore)
            {
                _rebrickableClient = rebrickableClient;
                _aggregateStore = aggregateStore;
            }

            public async Task<RebrickableSetAggregate> HandleAsync(SyncSetRebrickableDataCommand command)
            {
                var set = await _aggregateStore.GetByIdOrDefaultAsync<RebrickableSetAggregate>(command.Id);

                var apiSet = await _rebrickableClient.LegoSetsReadAsync(command.Id);
                set.SetData(apiSet);

                var minifigures = await _rebrickableClient.LegoSetsMinifigsListAsync(command.Id);
                foreach (var minifigure in minifigures.Results)
                {
                    set.SetMinifigureData(minifigure);
                }

                return set;
            }
        }
    }
}