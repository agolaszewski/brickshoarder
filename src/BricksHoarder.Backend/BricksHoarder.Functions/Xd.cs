using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Azure.WebJobs;

namespace BricksHoarder.Functions;

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