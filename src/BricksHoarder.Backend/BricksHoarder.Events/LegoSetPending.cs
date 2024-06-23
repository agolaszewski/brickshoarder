using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Events;
using BricksHoarder.Core.Helpers;

namespace BricksHoarder.Events
{
    public record LegoSetPending(string SetId, DateTime PendingUntil) : IEvent, IScheduling<SyncSetLegoDataCommand>
    {
        public SchedulingDetails<SyncSetLegoDataCommand> SchedulingDetails()
        {
            return new SchedulingDetails<SyncSetLegoDataCommand>($"SyncSetLegoDataCommand-{SetId}", new SyncSetLegoDataCommand(SetId), PathHelper.QueuePathUri<SyncSetLegoDataCommand>(), PendingUntil);
        }
    }
}