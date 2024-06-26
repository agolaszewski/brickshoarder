using BricksHoarder.Core.Events;
using BricksHoarder.Events;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SetReleasedFunction : BaseBatchFunction
{
    public SetReleasedFunction(IEventDispatcher eventDispatcher) : base(eventDispatcher)
    {
    }

    [Function(SetReleasedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SetReleasedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString, IsBatched = true)] ServiceBusReceivedMessage[] @events, CancellationToken cancellationToken)
    {
        await HandleBatchAsync<SetReleased>(@events);
    }
}