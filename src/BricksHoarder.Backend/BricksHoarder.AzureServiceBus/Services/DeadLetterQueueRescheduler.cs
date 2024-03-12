using Azure.Messaging.ServiceBus;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace BricksHoarder.Azure.ServiceBus.Services
{
    public class DeadLetterQueueRescheduler
    {
        private readonly IAzureClientFactory<ServiceBusClient> _serviceBusClientFactory;
        private readonly ILogger<DeadLetterQueueRescheduler> _logger;

        public DeadLetterQueueRescheduler(IAzureClientFactory<ServiceBusClient> serviceBusClientFactory, ILogger<DeadLetterQueueRescheduler> logger)
        {
            _serviceBusClientFactory = serviceBusClientFactory;
            _logger = logger;
        }

        public async Task HandleAsync(ServiceBusReceivedMessage message)
        {
            return;

            _logger.LogWarning("DeadLetterQueueRescheduler invoked");

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

            _logger.LogWarning("type : {0} MessageId : {1}", type, message.MessageId);

            ServiceBusClient serviceBusClient = _serviceBusClientFactory.CreateClient("ServiceBusClient");
            var client = serviceBusClient.CreateSender(type);
            await client.SendMessageAsync(new ServiceBusMessage(message));

            _logger.LogWarning("DeadLetterQueueRescheduler finished");
        }
    }
}