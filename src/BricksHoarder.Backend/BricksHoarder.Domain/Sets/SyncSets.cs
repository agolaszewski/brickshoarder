using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using RebrickableApi;

namespace BricksHoarder.Domain.Sets;

public class SyncSets
{
    public class Handler : ICommandHandler<SyncSetsCommand, SetsCollectionAggregate>
    {
        private readonly IRebrickableClient _rebrickableClient;
        private readonly IAggregateStore _aggregateStore;

        public Handler(IRebrickableClient rebrickableClient, IAggregateStore aggregateStore)
        {
            _rebrickableClient = rebrickableClient;
            _aggregateStore = aggregateStore;
        }

        public async Task<SetsCollectionAggregate> HandleAsync(SyncSetsCommand command)
        {
            var sets = await _aggregateStore.GetByIdOrDefaultAsync<SetsCollectionAggregate>();
            int page = 0;
            
            while (true)
            {
                page += 1;

                IReadOnlyList<LegoSetsListAsyncResponse.Result> setsFromApi = await GetSetsAsync(page);
                foreach (var apiSet in setsFromApi)
                {
                    if (sets.HasChanged(apiSet))
                    {
                        continue;
                    }

                    return sets;
                }
            }
        }

        private async Task<IReadOnlyList<LegoSetsListAsyncResponse.Result>> GetSetsAsync(int pageNumber)
        {
            LegoSetsListAsyncResponse? response = await _rebrickableClient.LegoSetsListAsync(page: pageNumber, page_size: 50, ordering: "-last_modified_dt");
            return response.Results;
        }
    }
}