using BricksHoarder.Core.Events;

namespace BricksHoarder.Azure.ServiceBus.Services
{
    public interface IRetryCommandService
    {
        RetryDetails? Get();
    }
}