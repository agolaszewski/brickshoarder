using BricksHoarder.Domain.Sets;
using BricksHoarder.Events.Metadata;
using MassTransit;
using Microsoft.Azure.WebJobs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BricksHoarder.Functions;

public class SyncSagaStartedFunction : BaseFunction
{
    private readonly IMessageReceiver _receiver;

    public SyncSagaStartedFunction(IMessageReceiver receiver)
    {
        _receiver = receiver;
    }

    [FunctionName(SyncSagaStartedMetadata.Consumer)]
    public async Task Run([ServiceBusTrigger(SyncSagaStartedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] Azure.Messaging.ServiceBus.ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        try
        {
            await _receiver.HandleSaga<SyncSetsState>(SyncSagaStartedMetadata.TopicPath, Default, @event, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}

public abstract class BaseFunction
{
    protected const string Default = "default";
    protected const string ServiceBusConnectionString = "ServiceBusConnectionString";
}