namespace BricksHoarder.Core.Services
{
    public interface IMessageLockService
    {
        bool Lock(string key, Guid lockedToMessageId, DateTime expireAtUtc);

        void Unlock(string key);
    }
}