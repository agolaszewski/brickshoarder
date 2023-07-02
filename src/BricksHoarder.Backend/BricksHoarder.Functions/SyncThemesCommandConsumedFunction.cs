using MassTransit;
using Microsoft.Azure.WebJobs;
using System.Threading;
using System.Threading.Tasks;
using BricksHoarder.Events.Metadata;
using BricksHoarder.Domain.Sets;

namespace BricksHoarder.Functions;

public class SyncThemesCommandConsumedFunction : BaseFunction
{
    public SyncThemesCommandConsumedFunction(IMessageReceiver receiver) : base(receiver)
    {
    }

    [FunctionName(SyncThemesCommandConsumedMetadata.Consumer)]
    public async Task Run([ServiceBusTrigger(SyncThemesCommandConsumedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await HandleSaga<SyncSetsSagaState>(@event, SyncThemesCommandConsumedMetadata.TopicPath, Default, cancellationToken);
    }
}