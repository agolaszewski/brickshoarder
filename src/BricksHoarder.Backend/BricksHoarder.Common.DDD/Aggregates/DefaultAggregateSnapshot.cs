using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Services;

namespace BricksHoarder.Common.DDD.Aggregates
{
    public class DefaultAggregateSnapshot<TAggregate> : IAggregateSnapshot<TAggregate> where TAggregate : class, IAggregateRoot
    {
        private ICacheService _cacheService;

        public DefaultAggregateSnapshot(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<TAggregate?> LoadAsync(string key)
        {
            return await _cacheService.GetAsync<TAggregate>(key);
        }

        public async Task SaveAsync(string streamName, TAggregate aggregate, TimeSpan timeSpan)
        {
            await _cacheService.SetAsync(streamName, aggregate, timeSpan);
        }
    }
}