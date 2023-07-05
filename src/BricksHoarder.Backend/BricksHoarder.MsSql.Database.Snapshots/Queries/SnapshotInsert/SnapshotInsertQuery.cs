using System.Data;
using BricksHoarder.MsSql.Database.Snapshots.Tables;
using Dapper;

namespace BricksHoarder.MsSql.Database.Snapshots.Queries.SnapshotInsert
{
    public class SnapshotInsertQuery
    {
        private readonly IDbConnection _dbConnection;
        private readonly SqlScripts _sqlScripts;

        public SnapshotInsertQuery(IDbConnection dbConnection, SqlScripts sqlScripts)
        {
            _dbConnection = dbConnection;
            _sqlScripts = sqlScripts;
        }

        public async Task ExecuteAsync(Snapshot item)
        {
            await _dbConnection.ExecuteAsync(_sqlScripts.SnapshotInsert, item);
        }
    }
}