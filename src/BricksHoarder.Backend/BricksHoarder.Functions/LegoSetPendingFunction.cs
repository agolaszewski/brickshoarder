using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Events;
using BricksHoarder.Events;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions;

public class LegoSetPendingFunction : BaseFunction
{
    public LegoSetPendingFunction(IMessageReceiver receiver, ILogger<LegoSetPendingFunction> logger) : base(receiver, logger)
    {
    }

    [Function(LegoSetPendingMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(LegoSetPendingMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await ScheduleAsync<SyncSetLegoDataCommand,LegoSetPending>(@event,LegoSetPendingMetadata.TopicPath, Default, cancellationToken);
    }
}