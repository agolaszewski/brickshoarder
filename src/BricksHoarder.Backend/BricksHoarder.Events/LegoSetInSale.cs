using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetInSale(string SetId, DateTime NextJobRun) : IEvent, IScheduling<SyncSetLegoDataCommand>
    {
        public SchedulingDetails<SyncSetLegoDataCommand> SchedulingDetails()
        {
            return new SchedulingDetails<SyncSetLegoDataCommand>($"SyncSetLegoDataCommand-{SetId}",new SyncSetLegoDataCommand(SetId), SyncSetLegoDataCommandMetadata.QueuePathUri, NextJobRun);
        }
    }
}