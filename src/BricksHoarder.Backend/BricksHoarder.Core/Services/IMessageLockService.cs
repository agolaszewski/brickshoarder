namespace BricksHoarder.Core.Services
{
    public interface IMessageLockService
    {
        bool Lock(string key, DateTime expireAtUtc);

        bool Check(string key);

        void Unlock(string key);
    }
}