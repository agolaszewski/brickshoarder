using BricksHoarder.Core.Services;
using Microsoft.Extensions.Caching.Memory;

namespace BricksHoarder.Cache.InMemory
{
    public class InMemoryCache : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public InMemoryCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Set<T>(string key, T value, TimeSpan? expire) where T : class
        {
            if (expire.HasValue)
            {
                _memoryCache.Set(key, value, expire.Value);
                return;
            }

            _memoryCache.Set(key, value);
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            var result = _memoryCache.Get<T>(key);
            return Task.FromResult<T?>(result);
        }
    }
}