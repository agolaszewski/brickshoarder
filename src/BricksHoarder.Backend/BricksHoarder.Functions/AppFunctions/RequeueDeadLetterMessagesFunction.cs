using Azure.Messaging.ServiceBus;
using BricksHoarder.Azure.ServiceBus.Services;
using BricksHoarder.Core.Commands;
using Microsoft.AspNetCore.Http;
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
        public async Task RunAsync([ServiceBusTrigger("brickshoarder/fault", "default", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions actions, CancellationToken cancellationToken)
        {
            if (!(await _deadLetterQueueRescheduler.HandleAsync(message)))
            {
                await actions.DeadLetterMessageAsync(message, cancellationToken: cancellationToken);
            }
        }
    }

    public class ResubmitDeadQueueFunction
    {
        private readonly ResubmitDeadQueueService _resubmitDeadQueueService;

        public ResubmitDeadQueueFunction(ResubmitDeadQueueService resubmitDeadQueueService)
        {
            _resubmitDeadQueueService = resubmitDeadQueueService;
        }

        [Function("ResubmitDeadQueueFunction")]
        public async Task RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            await _resubmitDeadQueueService.HandleAsync(int.Parse(req.Form["amount"]));
        }
    }
}