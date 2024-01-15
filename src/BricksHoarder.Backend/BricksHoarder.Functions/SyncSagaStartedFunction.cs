using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SyncSagaStartedFunction : BaseFunction
{
    public SyncSagaStartedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SyncSagaStartedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SyncSagaStartedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SyncSagaStartedMetadata.TopicPath, Default, cancellationToken);
    }
}