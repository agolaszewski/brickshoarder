using BricksHoarder.Core.Services;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Helpers;
using StackExchange.Redis;

namespace BricksHoarder.Redis
{
    public class MessageLockService(IDatabase cache, IDateTimeProvider dateTimeProvider) : IMessageLockService
    {
        public bool Lock(string key, System.DateTime expireAtUtc)
        {
            var expire = expireAtUtc - dateTimeProvider.UtcNow();
            if (expire.Ticks <= 0)
            {
                return false;
            }

            var isSuccess = cache.StringSet(key, string.Empty, expire, When.NotExists);
            return !isSuccess;
        }

        public bool Check(string key)
        {
            return cache.KeyExists(key);
        }

        public void Unlock(string key)
        {
            cache.KeyDelete(key);
        }
    }
}