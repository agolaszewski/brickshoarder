using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Domain.LegoSet;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SyncSetLegoDataCommandFunction : BaseFunction
{
    public SyncSetLegoDataCommandFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SyncSetLegoDataCommandMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SyncSetLegoDataCommandMetadata.QueuePath, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage command, CancellationToken cancellationToken)
    {
        await HandleCommandAsync<SyncSetLegoDataCommand, LegoSetAggregate>(command, cancellationToken);
    }
}