using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SyncSetRebrickableDataCommandConsumedFunction : BaseFunction
{
    public SyncSetRebrickableDataCommandConsumedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SyncSetRebrickableDataCommandConsumedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SyncSetRebrickableDataCommandConsumedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SyncSetRebrickableDataCommandConsumedMetadata.TopicPath, Default, cancellationToken);
    }
}