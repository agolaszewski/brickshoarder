using BricksHoarder.Commands.Themes;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using RebrickableApi;

namespace BricksHoarder.Domain.Themes;

public class SyncThemes
{
    public class Handler : ICommandHandler<SyncThemesCommand>
    {
        private readonly IRebrickableClient _rebrickableClient;
        private readonly IAggregateStore _aggregateStore;

        public Handler(IRebrickableClient rebrickableClient, IAggregateStore aggregateStore)
        {
            _rebrickableClient = rebrickableClient;
            _aggregateStore = aggregateStore;
        }

        public async Task<IAggregateRoot> ExecuteAsync(SyncThemesCommand command)
        {
            var themesFromApi = await GetAllThemes();
            var collection = await _aggregateStore.GetByIdOrDefaultAsync<ThemesCollectionAggregate>();

            foreach (var themeApi in themesFromApi)
            {
                collection.Add(themeApi);
            }

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

                await Task.Delay(TimeSpan.FromSeconds(2));
                collection.AddRange(response.Results);

                hasMore = !string.IsNullOrWhiteSpace(response.Next);
                pageNumber++;
            }
            while (hasMore);

            return collection;
        }
    }
}