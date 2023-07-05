using System.Text.Json;
using BricksHoarder.Core.Services;
using BricksHoarder.DateTime;
using BricksHoarder.MsSql.Database.Snapshots.Queries.SnapshotInsert;

namespace BricksHoarder.Cache.MsSql
{
    public class MsSqlCache : ICacheService
    {
        private readonly SnapshotInsertQuery _snapshotInsertQuery;

        public MsSqlCache(CacheInsertQuery cacheInsertQuery, IDateTimeProvider dateTimeProvider)
        {
            _snapshotInsertQuery = cacheInsertQuery;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expire) where T : class
        {
            var jsonValue = JsonSerializer.Serialize(value);
            await _cacheInsertQuery.ExecuteAsync(new BricksHoarder.MsSql.Database.BricksHoarder.Tables.Cache(key, jsonValue, null));
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            return null;
        }
    }
}