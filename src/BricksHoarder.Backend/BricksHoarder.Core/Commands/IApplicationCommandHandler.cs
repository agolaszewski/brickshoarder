using BricksHoarder.Core.Events;

namespace BricksHoarder.Core.Commands
{
    public interface IApplicationCommandHandler<in TCommand> where TCommand : ICommand
    {
        Task<IEnumerable<IEvent>> ExecuteAsync(TCommand command);
    }
}