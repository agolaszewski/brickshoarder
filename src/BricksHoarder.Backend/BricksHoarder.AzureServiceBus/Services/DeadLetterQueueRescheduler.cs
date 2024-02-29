using Azure.Messaging.ServiceBus;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using Microsoft.Extensions.Azure;
using System.Text;
using System.Text.Json;

namespace BricksHoarder.Azure.ServiceBus.Services
{
    public class DeadLetterQueueRescheduler
    {
        private readonly IAzureClientFactory<ServiceBusClient> _serviceBusClientFactory;

        public DeadLetterQueueRescheduler(IAzureClientFactory<ServiceBusClient> serviceBusClientFactory)
        {
            _serviceBusClientFactory = serviceBusClientFactory;
        }

        public async Task HandleAsync(ServiceBusReceivedMessage message)
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

                if (messageType.Any(e => e.Contains("BatchEvent")))
                {
                    type = $"brickshoarder.events/{name}";
                }
                else if (messageType.Any(e => e.Contains("CommandConsumed")))
                {
                    type = $"brickshoarder.events/consumed/{name}";
                }
                else if (messageType.Any(e => e.Contains("BatchEvent")))
                {
                    type = $"brickshoarder.events/batch/{name}";
                }
            }

            ServiceBusClient serviceBusClient = _serviceBusClientFactory.CreateClient("ServiceBusClient");
            var client = serviceBusClient.CreateSender(type);
            await client.SendMessageAsync(new ServiceBusMessage(message));
        }
    }
}