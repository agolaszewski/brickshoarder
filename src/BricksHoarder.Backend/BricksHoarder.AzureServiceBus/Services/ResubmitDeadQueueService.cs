using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Azure.ServiceBus.Services
{
    public class ResubmitDeadQueueService(IAzureClientFactory<ServiceBusClient> serviceBusClientFactory, ILogger<ResubmitDeadQueueService> logger)
    {
        private readonly ServiceBusClient _serviceBusClient = serviceBusClientFactory.CreateClient("ServiceBusClient");

        public async Task HandleAsync(int amount)
        {
            logger.LogWarning("ResubmitDeadQueueService invoked");

            var receiver = _serviceBusClient.CreateReceiver("brickshoarder/fault", "default", new ServiceBusReceiverOptions()
            {
                SubQueue = SubQueue.DeadLetter
            });

            var messages = await receiver.ReceiveMessagesAsync(amount);
            foreach (var message in messages)
            {
                await ResendAsync(message);
                await receiver.CompleteMessageAsync(message);
            }
        }

        private async Task ResendAsync(ServiceBusReceivedMessage message)
        {
            var body = Encoding.UTF8.GetString(message.Body);

            using var jsonParse = JsonDocument.Parse(body);
            var messageType = jsonParse.RootElement.GetProperty("messageType")
                .Deserialize<List<string>>()!
                .Select(x => x.Replace("urn:message:", string.Empty)).ToList().AsReadOnly();

            string type = null;
            if (messageType.Any(e => e.Contains(nameof(ICommand))))
            {
                type = messageType[0].Split(":")[1];
            }

            if (messageType.Any(e => e.Contains(nameof(IEvent))))
            {
                var name = messageType[0].Split(":").Last().Replace("]", string.Empty);

                if (messageType.Any(e => e.Contains("CommandConsumed")))
                {
                    type = $"brickshoarder.events/consumed/{name}";
                }
                else if (messageType.Any(e => e.Contains("BatchEvent")))
                {
                    type = $"brickshoarder.events/batch/{name}";
                }
                else
                {
                    type = $"brickshoarder.events/{name}";
                }
            }

            logger.LogWarning("ResubmitDeadQueueService type : {type} MessageId : {messageId}", type, message.MessageId);

            var client = _serviceBusClient.CreateSender(type);
            await client.SendMessageAsync(new ServiceBusMessage(message));

            logger.LogWarning("DeadLetterQueueRescheduler finished");
        }
    }
}