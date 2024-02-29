using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using System.Text;
using System.Text.Json;

namespace BricksHoarder.Functions.AppFunctions
{
    public class RequeueDeadLetterMessagesFunction
    {
        private readonly IAzureClientFactory<ServiceBusClient> _serviceBusClientFactory;

        public RequeueDeadLetterMessagesFunction(IAzureClientFactory<ServiceBusAdministrationClient> serviceBusAdministrationClientFactory, IAzureClientFactory<ServiceBusClient> serviceBusClientFactory)
        {
            _serviceBusClientFactory = serviceBusClientFactory;
        }

        [Function("DeadLetterQueueCommands")]
        public async Task RunAsync([ServiceBusTrigger("masstransit/fault2", "default", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
        {
            
        }
    }
}