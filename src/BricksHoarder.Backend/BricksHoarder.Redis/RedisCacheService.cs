using BricksHoarder.Core.Services;
using MessagePack;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using StackExchange.Redis;
using System.Text.Json;
using static MassTransit.ValidationResultExtensions;

namespace BricksHoarder.Redis
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _cache;

        private static readonly JsonSerializerOptions SerializeOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        static RedisCacheService()
        {
            SerializeOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        public RedisCacheService(IDatabase cache)
        {
            _cache = cache;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expire) where T : class
        {
            byte[] data = MessagePackSerializer.Typeless.Serialize(value);
            await _cache.StringSetAsync(key, data, expire);
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            RedisValue result = await _cache.StringGetAsync(key);
            if (result.HasValue)
            {
                byte[] bytes = result;
                T? obj = MessagePackSerializer.Typeless.Deserialize(bytes) as T;
                return obj;
            }

            return null;
        }
    }
}