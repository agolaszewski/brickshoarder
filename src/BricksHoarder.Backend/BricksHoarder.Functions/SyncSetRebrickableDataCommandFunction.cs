using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Domain.RebrickableSet;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions;

public class SyncSetRebrickableDataCommandFunction : BaseFunction
{
    public SyncSetRebrickableDataCommandFunction(IMessageReceiver receiver, ILogger<SyncSetRebrickableDataCommandFunction> logger) : base(receiver, logger)
    {
    }

    [Function(SyncSetRebrickableDataCommandMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SyncSetRebrickableDataCommandMetadata.QueuePath, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage command, CancellationToken cancellationToken)
    {
        await HandleCommandAsync<SyncSetRebrickableDataCommand, RebrickableSetAggregate>(command, cancellationToken);
    }
}