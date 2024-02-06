using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Events;
using Rebrickable.Api;

namespace BricksHoarder.Domain.SetsCollection;

public class SyncSets
{
    public class Handler : ICommandHandler<SyncSetsCommand, SetsCollectionAggregate>
    {
        private readonly IRebrickableClient _rebrickableClient;
        private readonly IAggregateStore _aggregateStore;
        private readonly IIntegrationEventsQueue _integrationEventsQueue;

        public Handler(IRebrickableClient rebrickableClient, IAggregateStore aggregateStore, IIntegrationEventsQueue integrationEventsQueue)
        {
            _rebrickableClient = rebrickableClient;
            _aggregateStore = aggregateStore;
            _integrationEventsQueue = integrationEventsQueue;
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
                    sets.HasChanged(apiSet);
                }

                if (!sets.Events.Any())
                {
                    _integrationEventsQueue.Queue(new NoChangesToSets());
                }

                return sets;
            }
        }

        private async Task<IReadOnlyList<LegoSetsListAsyncResponse.Result>> GetSetsAsync(int pageNumber)
        {
            LegoSetsListAsyncResponse? response = await _rebrickableClient.LegoSetsListAsync(page: pageNumber, page_size: 1000, ordering: "-last_modified_dt");
            return response.Results;
        }
    }
}