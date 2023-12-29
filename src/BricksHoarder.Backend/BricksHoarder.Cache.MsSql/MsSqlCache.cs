using BricksHoarder.Core.Services;
using MessagePack;
using Microsoft.Extensions.Caching.Distributed;

namespace BricksHoarder.Cache.MsSql
{
    public class MsSqlCache : ICacheService
    {
        private readonly IDistributedCache _distributedCache;

        public MsSqlCache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expire) where T : class
        {
            byte[] data = MessagePackSerializer.Typeless.Serialize(value);
            await _distributedCache.SetAsync(key, data, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expire
            });
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var data = await _distributedCache.GetAsync(key);
            if (data != null)
            {
                T? obj = MessagePackSerializer.Typeless.Deserialize(data) as T;
                return obj;
            }

            return null;
        }
    }
}