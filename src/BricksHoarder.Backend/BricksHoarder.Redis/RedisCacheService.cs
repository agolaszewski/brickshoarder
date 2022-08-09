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
            string json = JsonSerializer.Serialize(value, SerializeOptions);
            await _cache.StringSetAsync(key, json, expire);
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            RedisValue value = await _cache.StringGetAsync(key);
            return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<T>(value, SerializeOptions);
        }
    }
}