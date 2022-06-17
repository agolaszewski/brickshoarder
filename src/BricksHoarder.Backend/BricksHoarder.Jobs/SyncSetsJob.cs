using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Jobs;
using RebrickableApi;

namespace BricksHoarder.Jobs;

public class SyncSetsJob : IJob<SyncSetsJobInput>
{
    private readonly IRebrickableClient _rebrickableClient;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IAggregateStore _aggregateStore;

    public SyncSetsJob(IRebrickableClient rebrickableClient, ICommandDispatcher commandDispatcher, IAggregateStore aggregateStore)
    {
        _rebrickableClient = rebrickableClient;
        _commandDispatcher = commandDispatcher;
        _aggregateStore = aggregateStore;
    }

    public async Task RunAsync(SyncSetsJobInput input)
    {
        var response = await _rebrickableClient.LegoSetsListAsync(page: input.PageNumber, page_size: 100, ordering: "year", min_year:2015);
        IReadOnlyList<Result> sets = response.Results.Where(set => set.NumParts > 20).ToList();
        foreach (var set in sets)
        {
            //var setAggregate = await _aggregateStore.GetByIdOrDefaultAsync<SetAggregate>(set.Name);
            if (set.SetNum.EndsWith("2") || set.SetNum == "213-1")
            {
                int x = 1;
            }
        }

        await RunAsync(new SyncSetsJobInput() { PageNumber = input.PageNumber + 1 });
    }
}