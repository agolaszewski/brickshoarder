using BricksHoarder.Core.Database;
using Dapper;
using System.Data;

namespace BricksHoarder.MsSql.Database.Queries.CleanCache
{
    public class CleanCache : IDbQuery
    {
        private readonly IDbConnection _dbConnection;
        private readonly SqlScripts _sqlScripts;

        public CleanCache(IDbConnection dbConnection, SqlScripts sqlScripts)
        {
            _dbConnection = dbConnection;
            _sqlScripts = sqlScripts;
        }

        public async Task ExecuteAsync()
        {
            await _dbConnection.ExecuteAsync(_sqlScripts.CacheClean);
        }
    }
}