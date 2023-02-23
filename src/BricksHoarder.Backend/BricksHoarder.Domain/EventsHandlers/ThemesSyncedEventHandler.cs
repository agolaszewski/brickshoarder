using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Events;

namespace BricksHoarder.Domain.EventsHandlers
{
    public class ThemesSyncedEventHandler : IEventHandler<ThemesSynced>
    {
        public Task<IReadOnlyList<ICommand>> HandleAsync(ThemesSynced @event)
        {
            IReadOnlyList<ICommand> command = new List<ICommand>() { new SyncSetsCommand() };
            return Task.FromResult(command);
        }
    }
}