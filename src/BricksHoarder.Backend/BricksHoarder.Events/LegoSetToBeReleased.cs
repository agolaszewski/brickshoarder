using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetToBeReleased(string SetId, DateTime ReleaseDate) : IEvent, IScheduling<SyncSetLegoDataCommand>
    {
        public SchedulingDetails<SyncSetLegoDataCommand> SchedulingDetails()
        {
            return new SchedulingDetails<SyncSetLegoDataCommand>($"SyncSetLegoDataCommand-{SetId}", new SyncSetLegoDataCommand(SetId), SyncSetLegoDataCommandMetadata.QueuePathUri, ReleaseDate);
        }
    }
}