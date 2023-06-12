using BricksHoarder.Commands.Themes;
using MassTransit;
using Microsoft.Azure.WebJobs;
using System.Threading;
using System.Threading.Tasks;

namespace BricksHoarder.Functions;

public class SyncThemesCommandFunction : BaseFunction
{
    public SyncThemesCommandFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [FunctionName(SyncThemesCommandMetadata.Consumer)]
    public async Task Run([ServiceBusTrigger(SyncThemesCommandMetadata.QueuePath, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage command, CancellationToken cancellationToken)
    {
        await HandleCommand<SyncThemesCommand>(command, cancellationToken);
    }
}