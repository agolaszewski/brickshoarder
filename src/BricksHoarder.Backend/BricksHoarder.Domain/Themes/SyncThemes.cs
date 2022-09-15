using BricksHoarder.Commands.Themes;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Events;
using BricksHoarder.Helpers;
using RebrickableApi;

namespace BricksHoarder.Domain.Themes;

public class SyncThemes
{
    public class Handler : ICommandHandler<SyncThemesCommand>
    {
        private readonly IRebrickableClient _rebrickableClient;
        private readonly IAggregateStore _aggregateStore;
        private readonly IIntegrationEventsQueue _integrationEventsQueue;

        public Handler(IRebrickableClient rebrickableClient, IAggregateStore aggregateStore,IIntegrationEventsQueue integrationEventsQueue)
        {
            _rebrickableClient = rebrickableClient;
            _aggregateStore = aggregateStore;
            _integrationEventsQueue = integrationEventsQueue;
        }

        public async Task<IAggregateRoot> ExecuteAsync(SyncThemesCommand command)
        {
            var themesFromApi = await GetAllThemes();
            var collection = await _aggregateStore.GetByIdOrDefaultAsync<ThemesCollectionAggregate>();

            foreach (var themeApi in themesFromApi)
            {
                collection.Add(themeApi);
            }

            _integrationEventsQueue.Queue(new ThemesSynced());

            return collection;
        }

        public async Task<IReadOnlyList<LegoThemesListAsyncResponse.Result>> GetAllThemes()
        {
            List<LegoThemesListAsyncResponse.Result> collection = new();
            int pageNumber = 1;
            bool hasMore;

            do
            {
                var response = await _rebrickableClient.LegoThemesListAsync(page: pageNumber, page_size: 1000, ordering: "id");
                collection.AddRange(response.Results);

                hasMore = !response.Next.IsNullOrWhiteSpace();
                pageNumber++;
            }
            while (hasMore);

            return collection;
        }
    }
}