using BricksHoarder.Core.Aggregates;

namespace BricksHoarder.Core.Commands
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task<IAggregateRoot> ExecuteAsync(TCommand command);
    }
}
