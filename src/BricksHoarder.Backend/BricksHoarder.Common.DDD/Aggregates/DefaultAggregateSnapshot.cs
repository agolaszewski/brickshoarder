using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Services;

namespace BricksHoarder.Common.DDD.Aggregates
{
    public class DefaultAggregateSnapshot<TAggragate> : IAggragateSnapshot<TAggragate> where TAggragate : class, IAggregateRoot
    {
        private ICacheService _cacheService;

        public DefaultAggregateSnapshot(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<TAggragate?> LoadAsync(string key)
        {
            return await _cacheService.GetAsync<TAggragate>(key);
        }

        public async Task SaveAsync(string streamName, TAggragate aggregate, TimeSpan timeSpan)
        {
            await _cacheService.SetAsync(streamName, aggregate, timeSpan);
        }
    }
}