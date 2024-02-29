using Azure.Messaging.ServiceBus;
using BricksHoarder.Azure.ServiceBus.Services;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions.AppFunctions
{
    public class RequeueDeadLetterMessagesFunction
    {
        private readonly DeadLetterQueueRescheduler _deadLetterQueueRescheduler;

        public RequeueDeadLetterMessagesFunction(DeadLetterQueueRescheduler deadLetterQueueRescheduler)
        {
            _deadLetterQueueRescheduler = deadLetterQueueRescheduler;
        }

        [Function("RequeueDeadLetterMessagesFunction")]
        public async Task RunAsync([ServiceBusTrigger("brickshoarder/fault", "default", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, CancellationToken cancellationToken)
        {
            await _deadLetterQueueRescheduler.HandleAsync(message);
        }
    }
}