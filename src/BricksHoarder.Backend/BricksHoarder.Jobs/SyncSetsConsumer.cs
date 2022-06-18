using BricksHoarder.Core.Aggregates;
using BricksHoarder.Domain.Sets;
using MassTransit;
using RebrickableApi;

namespace BricksHoarder.Jobs;

public class SyncSetsConsumer : IConsumer<SyncSetsCommand>
{
    private readonly IRebrickableClient _rebrickableClient;
    private readonly IAggregateStore _aggregateStore;

    public SyncSetsConsumer(IRebrickableClient rebrickableClient, IAggregateStore aggregateStore)
    {
        _rebrickableClient = rebrickableClient;
        _aggregateStore = aggregateStore;
    }

    public async Task Consume(ConsumeContext<SyncSetsCommand> context)
    {
        var command = context.Message;
        var response = await _rebrickableClient.LegoSetsListAsync(page: command.PageNumber, page_size: 1000, ordering: "year");
        IReadOnlyList<LegoSetsListAsyncResponse.Result> sets = response.Results.Where(set => set.NumParts > 1).ToList();

        foreach (var set in sets)
        {
            if (set.SetNum.EndsWith("2") || set.SetNum == "213-1")
            {
                var setAggregate = await _aggregateStore.GetByIdOrDefaultAsync<SetAggregate>(set.Name);
            }
        }
    }
}