using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Domain.LegoSet;
using MassTransit;

namespace BricksHoarder.Azure.ServiceBus
{
    public class SyncSetLegoDataCommandDefinition :
        ConsumerDefinition<CommandConsumer<SyncSetLegoDataCommand, LegoSetAggregate>>
    {
        public SyncSetLegoDataCommandDefinition()
        {
            EndpointName = SyncSetLegoDataCommandMetadata.QueuePath;
        }
    }
}