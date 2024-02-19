using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SetReleasedFunction : BaseFunction
{
    public SetReleasedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SetReleasedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SetReleasedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SetReleasedBatchMetadata.TopicPath, Default, cancellationToken);
    }
}