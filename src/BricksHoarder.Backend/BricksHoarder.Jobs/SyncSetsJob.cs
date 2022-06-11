using BricksHoarder.Core.Jobs;
using RebrickableApi;

namespace BricksHoarder.Jobs;

public class SyncSetsJob : IJob<SyncSetsJobInput>
{
    private readonly RebrickableClient _rebrickableClient;

    public SyncSetsJob(RebrickableClient rebrickableClient)
    {
        _rebrickableClient = rebrickableClient;
    }

    public async Task Run(SyncSetsJobInput input)
    {
        var response = await _rebrickableClient.LegoSetsListAsync(page: input.PageNumber, page_size: 100, ordering: "year");
        IReadOnlyList<Result> sets = response.Results.Where(set => set.NumParts > 0).ToList();
    }
}