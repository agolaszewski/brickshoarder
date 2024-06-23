using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;

namespace BricksHoarder.Functions.AppFunctions
{
    public class SendMassTransitMessage
    {
        private readonly IAzureClientFactory<ServiceBusClient> _serviceBusClientFactory;

        public SendMassTransitMessage(IAzureClientFactory<ServiceBusClient> serviceBusClientFactory)
        {
            _serviceBusClientFactory = serviceBusClientFactory;
        }

        [Function("SendMassTransitMessage")]
        public async Task RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            var serviceBusClient = _serviceBusClientFactory.CreateClient("ServiceBusClient");
            var sender = serviceBusClient.CreateSender(req.Headers["QueueOrTopicName"]);
            await sender.SendMessageAsync(new ServiceBusMessage(await BinaryData.FromStreamAsync(req.Body)));
        }
    }
}