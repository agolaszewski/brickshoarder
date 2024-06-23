using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Events;
using BricksHoarder.Events;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions;

public class LegoSetToBeReleasedFunction : BaseFunction
{
    public LegoSetToBeReleasedFunction(IMessageReceiver receiver, ILogger<LegoSetToBeReleasedFunction> logger) : base(receiver, logger)
    {
    }

    [Function(LegoSetToBeReleasedMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(LegoSetToBeReleasedMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await ScheduleAsync<SyncSetLegoDataCommand,LegoSetToBeReleased>(@event,LegoSetToBeReleasedMetadata.TopicPath, Default, cancellationToken);
    }
}