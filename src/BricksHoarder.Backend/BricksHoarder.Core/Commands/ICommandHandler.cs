using BricksHoarder.Core.Aggregates;

namespace BricksHoarder.Core.Commands
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task<IAggregateRoot> HandleAsync(TCommand command);
    }
}
