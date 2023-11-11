using BricksHoarder.MsSql.Database.Queries.CacheClean;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions
{
    public class CleanCacheDbFunction
    {
        private readonly CacheCleanQuery _cacheCleanQuery;

        public CleanCacheDbFunction(CacheCleanQuery cacheCleanQuery)
        {
            _cacheCleanQuery = cacheCleanQuery;
        }

        [Function("CleanCacheDbFunction")]
        public async Task Run([TimerTrigger("0 0 0/1 * * *", RunOnStartup = false)] TimerInfo trigger)
        {
            try
            {
                await _cacheCleanQuery.ExecuteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}