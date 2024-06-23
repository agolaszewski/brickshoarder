using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions;

public class SetDetailsChangedBatchFunction : BaseFunction
{
    public SetDetailsChangedBatchFunction(IMessageReceiver receiver, ILogger<SetDetailsChangedBatchFunction> logger) : base(receiver, logger)
    {
    }

    [Function(SetDetailsChangedBatchMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SetDetailsChangedBatchMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SetDetailsChangedBatchMetadata.TopicPath, Default, cancellationToken);
    }
}