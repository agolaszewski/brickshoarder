using MassTransit;
using Microsoft.Azure.WebJobs;
using System.Threading;
using System.Threading.Tasks;
using BricksHoarder.Events.Metadata;
using BricksHoarder.Domain.Sets;

namespace BricksHoarder.Functions;

public class ThemeAddedFunction : BaseFunction
{
    public ThemeAddedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [FunctionName(ThemeAddedMetadata.Consumer)]
    public async Task Run([ServiceBusTrigger(ThemeAddedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSaga<SyncSetsSagaState>(@event, ThemeAddedMetadata.TopicPath, Default, cancellationToken);
    }
}