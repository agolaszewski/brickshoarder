using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SyncThemesCommandFaultedFunction : BaseFunction
{
    public SyncThemesCommandFaultedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SyncThemesCommandFaultedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SyncThemesCommandFaultedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SyncThemesCommandFaultedMetadata.TopicPath, Default, cancellationToken);
    }
}