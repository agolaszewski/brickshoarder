using BricksHoarder.Core.Database;
using BricksHoarder.DateTime;
using BricksHoarder.MsSql.Database.Tables;
using Dapper;
using System.Data;

namespace BricksHoarder.MsSql.Database.Queries.CacheInsert
{
    public class CacheInsertQuery : IDbQuery
    {
        private readonly IDbConnection _dbConnection;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly SqlScripts _sqlScripts;

        public CacheInsertQuery(IDbConnection dbConnection, IDateTimeProvider dateTimeProvider, SqlScripts sqlScripts)
        {
            _dbConnection = dbConnection;
            _dateTimeProvider = dateTimeProvider;
            _sqlScripts = sqlScripts;
        }

        public async Task ExecuteAsync(string key, string value, TimeSpan? ttl)
        {
            System.DateTime? expiryAt = null;
            if (ttl.HasValue)
            {
                expiryAt = _dateTimeProvider.UtcNow().Add(ttl.Value);
            }

            var item = new Cache(key, value, expiryAt);
            await _dbConnection.ExecuteAsync(_sqlScripts.CacheInsert, item);
        }
    }
}