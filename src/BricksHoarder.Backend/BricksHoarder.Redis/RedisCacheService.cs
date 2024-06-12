using BricksHoarder.Core.Services;
using MessagePack;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using StackExchange.Redis;
using System.Text.Json;

namespace BricksHoarder.Redis
{
    public class RedisCacheService(IDatabase cache, ConnectionMultiplexer connection) : ICacheService
    {
        private static readonly JsonSerializerOptions SerializeOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        static RedisCacheService()
        {
            SerializeOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expire) where T : class
        {
            byte[] data = MessagePackSerializer.Typeless.Serialize(value);
            await cache.StringSetAsync(key, data, expire);
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            RedisValue result = await cache.StringGetAsync(key);
            if (result.HasValue)
            {
                byte[] bytes = result!;
                T? obj = MessagePackSerializer.Typeless.Deserialize(bytes) as T;
                return obj;
            }

            return null;
        }

        public async Task ClearAsync()
        {
            var servers = connection.GetServers();
            foreach (var server in servers)
            {
                await server.FlushAllDatabasesAsync();
            }
        }

        public async Task SetAsync(string key, System.DateTime value, TimeSpan? expire)
        {
            await cache.StringSetAsync(key, value.ToString("s", System.Globalization.CultureInfo.InvariantCulture), expire);
        }

        public async Task<T?> GetAsync<T>(string key, Func<string, T?> convertFn)
        {
            RedisValue result = await cache.StringGetAsync(key);
            if (result.HasValue)
            {
                return convertFn(result!);
            }

            return default;
        }
    }
}