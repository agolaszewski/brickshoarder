using BricksHoarder.Commands.Themes;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Helpers;
using RebrickableApi;

namespace BricksHoarder.Domain.Themes.Handlers;

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

        public async Task<IAggregateRoot> HandleAsync(SyncThemesCommand command)
        {
            var themesFromApi = await GetAllThemes();
            var themes = await _aggregateStore.GetByIdOrDefaultAsync<ThemesCollectionAggregate>();

            foreach (var themeApi in themesFromApi)
            {
                themes.Add(themeApi);
            }

            return themes;
        }

        private async Task<IReadOnlyList<LegoThemesListAsyncResponse.Result>> GetAllThemes()
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