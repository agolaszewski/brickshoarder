using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class NoChangesToSetsFunction : BaseFunction
{
    public NoChangesToSetsFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(NoChangesToSetsMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(NoChangesToSetsMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, NoChangesToSetsMetadata.TopicPath, Default, cancellationToken);
    }
}