using MassTransit;
using Microsoft.Azure.WebJobs;
using System;
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

    public class EventConsumerFunction
    {
        private readonly IMessageReceiver _receiver;

        public EventConsumerFunction(IMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        [FunctionName("EConsumer")]
        public async Task Run([ServiceBusTrigger("brickshoarder.events/themessynced", "default", Connection = "ServiceBusConnectionString")] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage command, CancellationToken cancellationToken)
        {
            try
            {
                await _receiver.Handle("brickshoarder.events/themessynced", "default", command, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public class Xd
    {
        private readonly IMessageReceiver _receiver;

        public Xd(IMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        [FunctionName("EConsumer2")]
        public async Task Run([ServiceBusTrigger("brickshoarder.events/syncsagastarted", "default", Connection = "ServiceBusConnectionString")] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage command, CancellationToken cancellationToken)
        {
            try
            {
                await _receiver.Handle("brickshoarder.events/syncsagastarted", "default", command, cancellationToken);
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