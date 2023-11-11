using BricksHoarder.Domain.Sets;
using BricksHoarder.Events.Metadata;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SyncSetsCommandConsumedFunction : BaseFunction
{
    public SyncSetsCommandConsumedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SyncSetsCommandConsumedMetadata.Consumer)]
    public async Task Run([ServiceBusTrigger(SyncSetsCommandConsumedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSaga<SyncSetsSagaState>(@event, SyncSetsCommandConsumedMetadata.TopicPath, Default, cancellationToken);
    }
}