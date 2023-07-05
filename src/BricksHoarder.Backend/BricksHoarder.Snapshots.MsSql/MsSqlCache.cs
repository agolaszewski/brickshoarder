using BricksHoarder.Core.Services;
using BricksHoarder.MsSql.Database.Snapshots.Queries.SnapshotGet;
using BricksHoarder.MsSql.Database.Snapshots.Queries.SnapshotInsert;
using BricksHoarder.MsSql.Database.Snapshots.Tables;
using System.Text.Json;

namespace BricksHoarder.Cache.InMemory
{
    public class MsSqlCache : ICacheService
    {
        private readonly SnapshotInsertQuery _snapshotInsertQuery;
        private readonly SnapshotGetQuery _snapshotGetQuery;

        public MsSqlCache(SnapshotInsertQuery snapshotInsertQuery, SnapshotGetQuery snapshotGetQuery)
        {
            _snapshotInsertQuery = snapshotInsertQuery;
            _snapshotGetQuery = snapshotGetQuery;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expire) where T : class
        {
            var jsonValue = JsonSerializer.Serialize(value);
            await _snapshotInsertQuery.ExecuteAsync(new Snapshot(key, jsonValue));
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var snapshot = await _snapshotGetQuery.ExecuteAsync(key);
            return snapshot is not null ? JsonSerializer.Deserialize<T>(snapshot.Value) : null;
        }
    }
}