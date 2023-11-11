using System.Text.Json;
using BricksHoarder.Core.Services;
using BricksHoarder.MsSql.Database.Queries.CacheGet;
using BricksHoarder.MsSql.Database.Queries.CacheInsert;

namespace BricksHoarder.Cache.MsSql
{
    public class MsSqlCache : ICacheService
    {
        private readonly CacheInsertQuery _cacheInsertQuery;
        private readonly CacheGetQuery _cacheGetQuery;

        public MsSqlCache(CacheInsertQuery cacheInsertQuery, CacheGetQuery cacheGetQuery)
        {
            _cacheInsertQuery = cacheInsertQuery;
            _cacheGetQuery = cacheGetQuery;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expire) where T : class
        {
            var jsonValue = JsonSerializer.Serialize(value);
            await _cacheInsertQuery.ExecuteAsync(key, jsonValue, expire);
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var cache = await _cacheGetQuery.ExecuteAsync(key);
            return cache != null ? JsonSerializer.Deserialize<T>(cache.Value) : null;
        }
    }
}