using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions;

public class SyncThemesCommandConsumedFunction : BaseFunction
{
    public SyncThemesCommandConsumedFunction(IMessageReceiver receiver, ILogger<SyncThemesCommandConsumedFunction> logger) : base(receiver, logger)
    {
    }

    [Function(SyncThemesCommandConsumedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SyncThemesCommandConsumedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSagaAsync<SyncRebrickableDataSagaState>(@event, SyncThemesCommandConsumedMetadata.TopicPath, Default, cancellationToken);
    }
}