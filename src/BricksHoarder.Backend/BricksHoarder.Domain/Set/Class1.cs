using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using Rebrickable.Api;

namespace BricksHoarder.Domain.Set
{
    public class FetchSetRebrickableData
    {
        public class Handler : ICommandHandler<FetchSetRebrickableDataCommand, RebrickableSetAggregate>
        {
            private readonly IRebrickableClient _rebrickableClient;
            private readonly IAggregateStore _aggregateStore;

            public Handler(IRebrickableClient rebrickableClient, IAggregateStore aggregateStore)
            {
                _rebrickableClient = rebrickableClient;
                _aggregateStore = aggregateStore;
            }

            public async Task<RebrickableSetAggregate> HandleAsync(FetchSetRebrickableDataCommand command)
            {
                var set = await _aggregateStore.GetByIdOrDefaultAsync<RebrickableSetAggregate>(command.Id);
                var apiSet = await _rebrickableClient.LegoSetsReadAsync(command.Id);

                set.SetValues(apiSet);

                return set;
            }
        }
    }
}