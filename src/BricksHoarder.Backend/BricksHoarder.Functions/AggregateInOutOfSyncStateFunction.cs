using MassTransit;
using Microsoft.Azure.WebJobs;
using System.Threading;
using System.Threading.Tasks;
using BricksHoarder.Events.Metadata;

namespace BricksHoarder.Functions;

public class AggregateInOutOfSyncStateFunction : BaseFunction
{
    public AggregateInOutOfSyncStateFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [FunctionName(AggregateInOutOfSyncStateMetadata.Consumer)]
    public Task Run([ServiceBusTrigger(AggregateInOutOfSyncStateMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}