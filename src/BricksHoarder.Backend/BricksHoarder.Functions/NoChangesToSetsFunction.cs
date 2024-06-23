using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions;

public class NoChangesToSetsFunction : BaseFunction
{
    public NoChangesToSetsFunction(IMessageReceiver receiver, ILogger<NoChangesToSetsFunction> logger) : base(receiver, logger)
    {
    }

    [Function(NoChangesToSetsMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(NoChangesToSetsMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, NoChangesToSetsMetadata.TopicPath, Default, cancellationToken);
    }
}