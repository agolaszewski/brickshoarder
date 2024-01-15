using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SyncThemesCommandConsumedFunction : BaseFunction
{
    public SyncThemesCommandConsumedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SyncThemesCommandConsumedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SyncThemesCommandConsumedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SyncThemesCommandConsumedMetadata.TopicPath, Default, cancellationToken);
    }
}