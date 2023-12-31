using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Domain.Set;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class FetchSetRebrickableDataCommandFunction : BaseFunction
{
    public FetchSetRebrickableDataCommandFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(FetchSetRebrickableDataCommandMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(FetchSetRebrickableDataCommandMetadata.QueuePath, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage command, CancellationToken cancellationToken)
    {
        await HandleCommandAsync<FetchSetRebrickableDataCommand,RebrickableSetAggregate>(command, cancellationToken);
    }
}