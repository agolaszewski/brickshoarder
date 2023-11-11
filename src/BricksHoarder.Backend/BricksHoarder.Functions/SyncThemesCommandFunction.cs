using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Themes;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SyncThemesCommandFunction : BaseFunction
{
    public SyncThemesCommandFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SyncThemesCommandMetadata.Consumer)]
    public async Task Run([ServiceBusTrigger(SyncThemesCommandMetadata.QueuePath, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage command, CancellationToken cancellationToken)
    {
        await HandleCommand<SyncThemesCommand>(command, cancellationToken);
    }
}