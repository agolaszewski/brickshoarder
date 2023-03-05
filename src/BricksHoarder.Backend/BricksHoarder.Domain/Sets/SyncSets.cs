using BricksHoarder.Commands.Themes;
using BricksHoarder.Common.CQRS;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Domain.Themes;
using BricksHoarder.Helpers;
using RebrickableApi;

namespace BricksHoarder.Domain.Sets
{
    public class SyncSets
    {
        public class Handler : ICommandHandler<SyncThemesCommand>
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

            public async Task<IAggregateRoot> HandleAsync(SyncThemesCommand command)
            {
                var themes = await _aggregateStore.GetByIdOrDefaultAsync<ThemesCollectionAggregate>();
                foreach (var theme in themes.Collection)
                {
                    var sets = GetAllSetsForTheme(theme.Id);
                    _integrationEventsQueue.Events;
                }
            }

            private async Task<IReadOnlyList<LegoSetsListAsyncResponse.Result>> GetAllSetsForTheme(int themeId)
            {
                List<LegoSetsListAsyncResponse.Result> collection = new();
                int pageNumber = 1;
                bool hasMore;

                do
                {
                    var response = await _rebrickableClient.LegoSetsListAsync(page: pageNumber, page_size: 1000, ordering: "id", theme_id: themeId.ToString());
                    collection.AddRange(response.Results);

                    hasMore = !response.Next.IsNullOrWhiteSpace();
                    pageNumber++;
                }
                while (hasMore);

                return collection;
            }
        }
    }
}