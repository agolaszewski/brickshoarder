using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.WebJobs;

namespace BricksHoarder.Functions;

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