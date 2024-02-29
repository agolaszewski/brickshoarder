using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using System.Transactions;

namespace BricksHoarder.Functions.Timers
{
    public class RequeueDeadLetterMessagesFunction
    {
        private readonly IAzureClientFactory<ServiceBusAdministrationClient> _serviceBusAdministrationClientFactory;
        private readonly IAzureClientFactory<ServiceBusClient> _serviceBusClientFactory;

        public RequeueDeadLetterMessagesFunction(IAzureClientFactory<ServiceBusAdministrationClient> serviceBusAdministrationClientFactory, IAzureClientFactory<ServiceBusClient> serviceBusClientFactory)
        {
            _serviceBusAdministrationClientFactory = serviceBusAdministrationClientFactory;
            _serviceBusClientFactory = serviceBusClientFactory;
        }

        [Function("DeadLetterQueue")]
        public async Task RunAsync([TimerTrigger("0 */5 * * * *")] TimerInfo trigger)
        {
            ServiceBusAdministrationClient serviceBusAdministrationClient = _serviceBusAdministrationClientFactory.CreateClient("ServiceBusAdministrationClient");
            var queues = serviceBusAdministrationClient.GetQueuesAsync();

            ServiceBusClient serviceBusClient = _serviceBusClientFactory.CreateClient("ServiceBusClient");
            await foreach (var queue in queues)
            {
                var messageReceiver = serviceBusClient.CreateReceiver($"{queue.Name}/$deadletterqueue", new ServiceBusReceiverOptions());
                IReadOnlyList<ServiceBusReceivedMessage> messages = await messageReceiver.ReceiveMessagesAsync(100, TimeSpan.FromSeconds(1));

                if (!messages.Any())
                {
                    continue;
                }

                var sender = serviceBusClient.CreateSender(queue.Name);

                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                foreach (var message in messages)
                {
                    var resubmitMessage = new ServiceBusMessage(message);
                    await sender.SendMessageAsync(resubmitMessage);
                    await messageReceiver.CompleteMessageAsync(message);
                }

                scope.Complete();

                await messageReceiver.DisposeAsync();
                await sender.DisposeAsync();
            }
        }
    }
}