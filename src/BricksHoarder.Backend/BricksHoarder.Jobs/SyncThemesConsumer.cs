using BricksHoarder.Core.Aggregates;
using BricksHoarder.Domain.Themes;
using MassTransit;
using RebrickableApi;

namespace BricksHoarder.Jobs;

public class SyncThemesConsumer : IConsumer<SyncThemesCommand>
{
    private readonly IRebrickableClient _rebrickableClient;
    private readonly IAggregateStore _aggregateStore;

    public SyncThemesConsumer(IRebrickableClient rebrickableClient, IAggregateStore aggregateStore)
    {
        _rebrickableClient = rebrickableClient;
        _aggregateStore = aggregateStore;
    }

    public async Task Consume(ConsumeContext<SyncThemesCommand> context)
    {
        var command = context.Message;
        var response = await _rebrickableClient.LegoThemesListAsync(page: command.PageNumber, page_size: 500, ordering: "id");

        var collection = await _aggregateStore.GetByIdOrDefaultAsync<ThemesCollectionAggregate>();
        foreach (var themeApi in response.Results)
        {
            var theme = collection.Get(themeApi.Id);

            if (theme is null)
            {
                theme.
            }
        }
    }
}