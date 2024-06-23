using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions;

public class SyncSetRebrickableDataCommandConsumedFunction : BaseFunction
{
    public SyncSetRebrickableDataCommandConsumedFunction(IMessageReceiver receiver, ILogger<SyncSetRebrickableDataCommandConsumedFunction> logger) : base(receiver, logger)
    {
    }

    [Function(SyncSetRebrickableDataCommandConsumedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SyncSetRebrickableDataCommandConsumedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SyncSetRebrickableDataCommandConsumedMetadata.TopicPath, Default, cancellationToken);
    }
}