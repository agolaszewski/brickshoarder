using BricksHoarder.Core.Database;
using BricksHoarder.DateTime;
using BricksHoarder.MsSql.Database.Tables;
using Dapper;
using System.Data;

namespace BricksHoarder.MsSql.Database.Queries.CacheGet
{
    public class CacheGetQuery : IDbQuery
    {
        private readonly IDbConnection _dbConnection;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly SqlScripts _sqlScripts;

        public CacheGetQuery(IDbConnection dbConnection, IDateTimeProvider dateTimeProvider, SqlScripts sqlScripts)
        {
            _dbConnection = dbConnection;
            _dateTimeProvider = dateTimeProvider;
            _sqlScripts = sqlScripts;
        }

        public async Task<Cache?> ExecuteAsync(string key)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<Cache>(_sqlScripts.CacheGet, new { key, Now = _dateTimeProvider.UtcNow() });
        }
    }
}