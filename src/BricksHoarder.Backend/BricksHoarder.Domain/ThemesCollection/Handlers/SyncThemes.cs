using BricksHoarder.Commands.Themes;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Helpers;
using Rebrickable.Api;

namespace BricksHoarder.Domain.ThemesCollection.Handlers;

public class SyncThemes
{
    public class Handler : ICommandHandler<SyncThemesCommand, ThemesCollectionAggregate>
    {
        private readonly IRebrickableClient _rebrickableClient;
        private readonly IAggregateStore _aggregateStore;

        public Handler(IRebrickableClient rebrickableClient, IAggregateStore aggregateStore)
        {
            _rebrickableClient = rebrickableClient;
            _aggregateStore = aggregateStore;
        }

        public async Task<ThemesCollectionAggregate> HandleAsync(SyncThemesCommand command)
        {
            Random r = new Random();
            if (r.Next(0, 100) < 95)
            {
                throw new Exception("UPS");
            }

            var themesFromApi = await GetAllThemesAsync();
            var themes = await _aggregateStore.GetByIdOrDefaultAsync<ThemesCollectionAggregate>();

            foreach (var themeApi in themesFromApi)
            {
                themes.Add(themeApi);
            }

            return themes;
        }

        private async Task<IReadOnlyList<LegoThemesListAsyncResponse.Result>> GetAllThemesAsync()
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