using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SetDetailsChangedFunction : BaseFunction
{
    public SetDetailsChangedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SetDetailsChangedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SetDetailsChangedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SetDetailsChangedMetadata.TopicPath, Default, cancellationToken);
    }
}