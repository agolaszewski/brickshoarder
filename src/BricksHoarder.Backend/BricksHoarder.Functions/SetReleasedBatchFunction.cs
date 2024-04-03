using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions;

public class SetReleasedBatchFunction : BaseFunction
{
    public SetReleasedBatchFunction(IMessageReceiver receiver, ILogger<SetReleasedBatchFunction> logger) : base(receiver, logger)
    {
    }

    [Function(SetReleasedBatchMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SetReleasedBatchMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SetReleasedBatchMetadata.TopicPath, Default, cancellationToken);
    }
}