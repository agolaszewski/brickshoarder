using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Core.Commands;
using BricksHoarder.Events;

namespace BricksHoarder.Domain.EventsHandlers
{
    public class ThemesSyncedEventHandler : Common.CQRS.CommandDispatcherEventHandler<ThemesSynced, SyncSetsCommand>
    {
        public ThemesSyncedEventHandler(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        public override SyncSetsCommand Dispatch(ThemesSynced @event)
        {
            return new SyncSetsCommand();
        }
    }
}