using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Services;

namespace BricksHoarder.Domain.Themes;

public class ThemesCollectionAggragateSnapshot : IAggragateSnapshot<ThemesCollectionAggregate>
{
    private ICacheService _cacheService;

    public ThemesCollectionAggragateSnapshot(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<ThemesCollectionAggregate?> LoadAsync(string key)
    {
        var snapshot = await _cacheService.GetAsync<ThemesCollectionSnapshot>(key);
        if (snapshot is null)
        {
            return null;
        }

        return new ThemesCollectionAggregate(snapshot);
    }

    public async Task SaveAsync(string streamName, ThemesCollectionAggregate aggregate, TimeSpan timeSpan)
    {
        var snapshot = new ThemesCollectionSnapshot(aggregate);
        await _cacheService.SetAsync(streamName, snapshot, timeSpan);
    }
}