using BricksHoarder.Domain.Sets;
using BricksHoarder.Events;
using MassTransit;
using Microsoft.Azure.WebJobs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BricksHoarder.Functions;

public class EventFunctions
{
    private readonly IMessageReceiver _receiver;

    public EventFunctions(IMessageReceiver receiver)
    {
        _receiver = receiver;
    }

    [FunctionName($"{nameof(SyncSagaStarted)}Consumer")]
    public async Task Run([ServiceBusTrigger("brickshoarder.events/syncsagastarted", "default", Connection = "ServiceBusConnectionString")] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        try
        {
            await _receiver.HandleSaga<SyncSetsState>("brickshoarder.events/syncsagastarted", "default", @event, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}

public class EventFunctions2
{
    private readonly IMessageReceiver _receiver;

    public EventFunctions2(IMessageReceiver receiver)
    {
        _receiver = receiver;
    }

    [FunctionName($"{nameof(CommandConsumed)}Consumer")]
    public async Task Run([ServiceBusTrigger("brickshoarder.events/consumed/syncthemescommand", "default", Connection = "ServiceBusConnectionString")] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        try
        {
            await _receiver.HandleSaga<SyncSetsState>("brickshoarder.events/consumed/syncthemescommand", "default", @event, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}