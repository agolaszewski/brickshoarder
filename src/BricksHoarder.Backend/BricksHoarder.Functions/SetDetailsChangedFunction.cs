using BricksHoarder.Core.Events;
using BricksHoarder.Events;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SetDetailsChangedFunction : BaseBatchFunction
{
    public SetDetailsChangedFunction(IEventDispatcher eventDispatcher) : base(eventDispatcher)
    {
    }

    [Function(SetDetailsChangedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SetDetailsChangedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString, IsBatched = true)] ServiceBusReceivedMessage[] @events, CancellationToken cancellationToken)
    {
        await HandleBatchAsync<SetDetailsChanged>(@events);
    }
}