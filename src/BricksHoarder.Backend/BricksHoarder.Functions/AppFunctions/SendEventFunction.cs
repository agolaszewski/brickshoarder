using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BricksHoarder.Core.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;

namespace BricksHoarder.Functions.AppFunctions
{
    public class SendEventFunction
    {
        private readonly IEventDispatcher _eventDispatcher;

        public SendEventFunction(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        [Function("SendEvent")]
        public async Task RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            var eventsAssembly = AppDomain.CurrentDomain.GetAssemblies().Single(assembly => assembly.GetName().Name == "BricksHoarder.Events");

            string typeFullName = req.Headers["Event-Type"]!;
            Type type = eventsAssembly.GetType(typeFullName)!;

            object @event = await JsonSerializer.DeserializeAsync(req.Body, type);
            await _eventDispatcher.DispatchAsync(@event);
        }
    }

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

