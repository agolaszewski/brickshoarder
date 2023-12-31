using BricksHoarder.Core.Aggregates;

namespace BricksHoarder.Core.Commands
{
    public interface ICommandHandler<in TCommand, TAggregateRoot> where TCommand : ICommand where TAggregateRoot : class, IAggregateRoot
    {
        Task<TAggregateRoot> HandleAsync(TCommand command);
    }

    public interface ICommandHandler<in TCommand> where TCommand : ICommand 
    {
        Task HandleAsync(TCommand command);
    }
}