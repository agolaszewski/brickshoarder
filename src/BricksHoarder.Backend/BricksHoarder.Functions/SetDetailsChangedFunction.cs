using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SetDetailsChangedFunction : BaseFunction
{
    public SetDetailsChangedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SetDetailsChangedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SetDetailsChangedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SetDetailsChangedMetadata.TopicPath, Default, cancellationToken);
    }
}