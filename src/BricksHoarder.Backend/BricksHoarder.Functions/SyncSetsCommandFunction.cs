using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Domain.SetsCollection;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SyncSetsCommandFunction : BaseFunction
{
    public SyncSetsCommandFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SyncSetsCommandMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SyncSetsCommandMetadata.QueuePath, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage command, CancellationToken cancellationToken)
    {
        await HandleCommandAsync<SyncSetsCommand, SetsCollectionAggregate>(command, cancellationToken);
    }
}