namespace BricksHoarder.Core.Services
{
    public interface ICacheService
    {
        void Set<T>(string key, T value, TimeSpan? expire) where T : class;

        Task<T?> GetAsync<T>(string key) where T : class;
    }
}