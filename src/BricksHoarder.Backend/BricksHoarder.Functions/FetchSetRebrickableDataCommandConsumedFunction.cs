using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class FetchSetRebrickableDataCommandConsumedFunction : BaseFunction
{
    public FetchSetRebrickableDataCommandConsumedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(FetchSetRebrickableDataCommandConsumedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(FetchSetRebrickableDataCommandConsumedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, FetchSetRebrickableDataCommandConsumedMetadata.TopicPath, Default, cancellationToken);
    }
}