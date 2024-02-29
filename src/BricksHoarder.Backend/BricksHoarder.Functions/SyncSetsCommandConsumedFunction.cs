using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SyncSetsCommandConsumedFunction : BaseFunction
{
    public SyncSetsCommandConsumedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SyncSetsCommandConsumedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SyncSetsCommandConsumedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SyncSetsCommandConsumedMetadata.TopicPath, Default, cancellationToken);
    }
}