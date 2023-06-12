using MassTransit;
using Microsoft.Azure.WebJobs;
using System.Threading;
using System.Threading.Tasks;
using BricksHoarder.Events.Metadata;
using BricksHoarder.Domain.Sets;

namespace BricksHoarder.Functions;

public class SyncSagaStartedFunction : BaseFunction
{
    public SyncSagaStartedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [FunctionName(SyncSagaStartedMetadata.Consumer)]
    public async Task Run([ServiceBusTrigger(SyncSagaStartedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSaga<SyncSetsSagaState>(@event, SyncSagaStartedMetadata.TopicPath, Default, cancellationToken);
    }
}