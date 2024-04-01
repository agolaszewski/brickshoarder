using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SetReleasedBatchFunction : BaseFunction
{
    public SetReleasedBatchFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SetReleasedBatchMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SetReleasedBatchMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SetReleasedBatchMetadata.TopicPath, Default, cancellationToken);
    }
}