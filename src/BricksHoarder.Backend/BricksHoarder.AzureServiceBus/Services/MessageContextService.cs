using BricksHoarder.Core.Events;
using MassTransit;

namespace BricksHoarder.Azure.ServiceBus.Services
{
    
    public class RetryCommandService(ConsumeContext consumeContext) : IRetryCommandService
    {
        public RetryDetails? Get()
        {
            var retryCount = consumeContext.Headers.Get<int>("MT-Retry-RetryCount");
            if (!retryCount.HasValue)
            {
                return null;
            }

            var originalOccurrenceDate = consumeContext.Headers.Get<System.DateTime>("MT-Retry-OriginalOccurrenceDate");
            var lastOccurrenceDate = consumeContext.Headers.Get<System.DateTime>("MT-Retry-LastOccurrenceDate");

            return new RetryDetails(retryCount!.Value, originalOccurrenceDate!.Value, lastOccurrenceDate!.Value);
        }
    }
}