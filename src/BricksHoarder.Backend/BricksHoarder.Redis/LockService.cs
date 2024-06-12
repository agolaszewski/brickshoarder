using BricksHoarder.Core.Services;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Helpers;
using StackExchange.Redis;

namespace BricksHoarder.Redis
{
    public class MessageLockService(IDatabase cache, IDateTimeProvider dateTimeProvider) : IMessageLockService
    {
        public bool Lock(string key, Guid lockedToMessageId, System.DateTime expireAtUtc)
        {
            var expire = expireAtUtc - dateTimeProvider.UtcNow();
            if (expire.Ticks <= 0)
            {
                return false;
            }

            var isSuccess = cache.StringSet(key, lockedToMessageId.ToString(), expire, When.NotExists);
            if (isSuccess)
            {
                return false;
            }

            var value = (string?)cache.StringGet(key);
            Guid? guid = value.ToGuid();
            return guid != lockedToMessageId;
        }

        public void Unlock(string key)
        {
            cache.KeyDelete(key);
        }
    }
}