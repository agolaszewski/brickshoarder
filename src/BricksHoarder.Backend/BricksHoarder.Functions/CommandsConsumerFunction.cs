using System;
using MassTransit;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BricksHoarder.Functions
{
    public class CommandsConsumerFunction
    {
        private readonly IMessageReceiver _receiver;

        public CommandsConsumerFunction(IMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        [FunctionName("CommandsConsumer")]
        public async Task Run([ServiceBusTrigger("commands", Connection = "ServiceBusConnectionString")] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage command, CancellationToken cancellationToken)
        {
            try
            {
                await _receiver.Handle("commands", command, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }

    //public class EventsConsumerFunction
    //{
    //    private readonly IMessageReceiver _receiver;

    //    public EventsConsumerFunction(IMessageReceiver receiver)
    //    {
    //        _receiver = receiver;
    //    }

    //    [FunctionName("EventsConsumer")]
    //    public async Task Run([ServiceBusTrigger("events", Connection = "ServiceBusConnectionString")] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    //    {
    //        await _receiver.Handle("events", @event, cancellationToken);
    //    }
    //}
}