using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Sets;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SyncSetsCommandFunction : BaseFunction
{
    public SyncSetsCommandFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SyncSetsCommandMetadata.Consumer)]
    public async Task Run([ServiceBusTrigger(SyncSetsCommandMetadata.QueuePath, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage command, CancellationToken cancellationToken)
    {
        await HandleCommand<SyncSetsCommand>(command, cancellationToken);
    }
}