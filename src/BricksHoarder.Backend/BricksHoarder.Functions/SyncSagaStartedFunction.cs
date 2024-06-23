using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions;

public class SyncSagaStartedFunction : BaseFunction
{
    public SyncSagaStartedFunction(IMessageReceiver receiver, ILogger<SyncSagaStartedFunction> logger) : base(receiver, logger)
    {
    }

    [Function(SyncSagaStartedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SyncSagaStartedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SyncSagaStartedMetadata.TopicPath, Default, cancellationToken);
    }
}