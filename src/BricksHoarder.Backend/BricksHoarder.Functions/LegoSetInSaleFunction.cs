using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Events;
using BricksHoarder.Events;
using BricksHoarder.Events.Metadata;
using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Functions;

public class LegoSetInSaleFunction : BaseFunction
{
    public LegoSetInSaleFunction(IMessageReceiver receiver, ILogger<LegoSetInSaleFunction> logger) : base(receiver, logger)
    {
    }

    [Function(LegoSetInSaleMetadata.Consumer)]
    public async Task RunAsync([ServiceBusTrigger(LegoSetInSaleMetadata.TopicPath, Default, Connection = ServiceBusConnectionString)] ServiceBusReceivedMessage @event, CancellationToken cancellationToken)
    {
        await ScheduleAsync<SyncSetLegoDataCommand,LegoSetInSale>(@event,LegoSetInSaleMetadata.TopicPath, Default, cancellationToken);
    }
}