using System.Data;
using BricksHoarder.Core.Database;
using BricksHoarder.DateTime;
using Dapper;

namespace BricksHoarder.MsSql.Database.Queries.CacheClean
{
    public class CacheCleanQuery : IDbQuery
    {
        private readonly IDbConnection _dbConnection;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly SqlScripts _sqlScripts;

        public CacheCleanQuery(IDbConnection dbConnection, IDateTimeProvider dateTimeProvider, SqlScripts sqlScripts)
        {
            _dbConnection = dbConnection;
            _dateTimeProvider = dateTimeProvider;
            _sqlScripts = sqlScripts;
        }

        public async Task ExecuteAsync()
        {
            await _dbConnection.ExecuteAsync(_sqlScripts.CacheClean, new { Time = _dateTimeProvider.UtcNow() });
        }
    }
}