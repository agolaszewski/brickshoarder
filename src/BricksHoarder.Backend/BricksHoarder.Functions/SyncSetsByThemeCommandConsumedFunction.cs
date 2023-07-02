using MassTransit;
using Microsoft.Azure.WebJobs;
using System.Threading;
using System.Threading.Tasks;
using BricksHoarder.Events.Metadata;
using BricksHoarder.Domain.Sets;

namespace BricksHoarder.Functions;

public class SyncSetsByThemeCommandConsumedFunction : BaseFunction
{
    public SyncSetsByThemeCommandConsumedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [FunctionName(SyncSetsByThemeCommandConsumedMetadata.Consumer)]
    public async Task Run([ServiceBusTrigger(SyncSetsByThemeCommandConsumedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSaga<SyncSetsSagaState>(@event, SyncSetsByThemeCommandConsumedMetadata.TopicPath, Default, cancellationToken);
    }
}