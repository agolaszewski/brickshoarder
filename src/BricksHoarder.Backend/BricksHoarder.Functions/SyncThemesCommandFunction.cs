using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Domain.ThemesCollection;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace BricksHoarder.Functions;

public class SyncThemesCommandFunction : BaseFunction
{
    public SyncThemesCommandFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [Function(SyncThemesCommandMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(SyncThemesCommandMetadata.QueuePath, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage command, CancellationToken cancellationToken)
    {
        await HandleCommandAsync<SyncThemesCommand, ThemesCollectionAggregate>(command, cancellationToken);
    }
}