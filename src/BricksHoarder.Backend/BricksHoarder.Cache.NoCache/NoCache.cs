using BricksHoarder.Core.Services;

namespace BricksHoarder.Cache.NoCache
{
    public class NoCache : ICacheService
    {
        public NoCache()
        {
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expire) where T : class
        {
            return Task.CompletedTask;
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            return Task.FromResult<T?>(null);
        }

        public Task ClearAsync()
        {
            return Task.CompletedTask;
        }
    }
}