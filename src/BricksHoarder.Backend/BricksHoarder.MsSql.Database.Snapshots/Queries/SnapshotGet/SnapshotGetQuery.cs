using System.Data;
using BricksHoarder.MsSql.Database.Snapshots.Tables;
using Dapper;

namespace BricksHoarder.MsSql.Database.Snapshots.Queries.SnapshotGet
{
    public class SnapshotGetQuery
    {
        private readonly IDbConnection _dbConnection;
        private readonly SqlScripts _sqlScripts;

        public SnapshotGetQuery(IDbConnection dbConnection, SqlScripts sqlScripts)
        {
            _dbConnection = dbConnection;
            _sqlScripts = sqlScripts;
        }

        public async Task<Snapshot?> ExecuteAsync(string key)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<Snapshot>(_sqlScripts.SnapshotGet, new { key });
        }
    }
}