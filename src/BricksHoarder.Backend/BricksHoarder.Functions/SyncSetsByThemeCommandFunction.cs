using BricksHoarder.Commands.Sets;
using MassTransit;
using Microsoft.Azure.WebJobs;
using System.Threading;
using System.Threading.Tasks;

namespace BricksHoarder.Functions;

public class SyncSetsByThemeCommandFunction : BaseFunction
{
    public SyncSetsByThemeCommandFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [FunctionName(SyncSetsByThemeCommandMetadata.Consumer)]
    public async Task Run([ServiceBusTrigger(SyncSetsByThemeCommandMetadata.QueuePath, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage command, CancellationToken cancellationToken)
    {
        await HandleCommand<SyncSetsByThemeCommand>(command, cancellationToken);
    }
}