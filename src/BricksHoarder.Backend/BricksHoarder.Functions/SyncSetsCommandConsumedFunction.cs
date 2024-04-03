using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions;

public class SyncSetsCommandConsumedFunction : BaseFunction
{
    public SyncSetsCommandConsumedFunction(IMessageReceiver receiver, ILogger<SyncSetsCommandConsumedFunction> logger) : base(receiver, logger)
    {
    }

    [Function(SyncSetsCommandConsumedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SyncSetsCommandConsumedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SyncSetsCommandConsumedMetadata.TopicPath, Default, cancellationToken);
    }
}