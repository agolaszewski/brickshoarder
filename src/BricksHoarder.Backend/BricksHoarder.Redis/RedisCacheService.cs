using BricksHoarder.Core.Services;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using StackExchange.Redis;
using System.Text.Json;

namespace BricksHoarder.Redis
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _cache;

        private static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions
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

        public void Set<T>(string key, T value, TimeSpan? expire) where T : class
        {
            string json = JsonSerializer.Serialize(value, SerializeOptions);
            _cache.StringSetAsync(key, json, expire);
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            RedisValue value = await _cache.StringGetAsync(key);
            return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<T>(value);
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> fallback, bool isForced, TimeSpan? expire) where T : class
        {
            if (isForced)
            {
                T result = await fallback();
                Set<T>(key, result, expire);
                return result;
            }

            RedisValueWithExpiry redisValue = _cache.StringGetWithExpiry(key);
            if (string.IsNullOrWhiteSpace(redisValue.Value))
            {
                var result = await fallback();
                Set<T>(key, result, expire);
                return result;
            }

            if (expire.HasValue && redisValue.Expiry?.TotalSeconds < expire.Value.TotalSeconds / 2)
            {
                _cache.KeyExpire(key, expire);
            }

            return JsonSerializer.Deserialize<T>(redisValue.Value, SerializeOptions);
        }
    }
}