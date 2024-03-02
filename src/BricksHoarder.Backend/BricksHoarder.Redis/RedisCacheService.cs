using BricksHoarder.Core.Services;
using MessagePack;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using StackExchange.Redis;
using System.Text.Json;

namespace BricksHoarder.Redis
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _cache;
        private readonly ConnectionMultiplexer _connection;
       
        private static readonly JsonSerializerOptions SerializeOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        static RedisCacheService()
        {
            SerializeOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        public RedisCacheService(IDatabase cache, ConnectionMultiplexer connection)
        {
            _cache = cache;
            _connection = connection;
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
                byte[] bytes = result!;
                T? obj = MessagePackSerializer.Typeless.Deserialize(bytes) as T;
                return obj;
            }

            return null;
        }

        public async Task ClearAsync()
        {
            var servers = _connection.GetServers();
            foreach (var server in servers)
            {
                await server.FlushAllDatabasesAsync();
            }
        }
    }
}