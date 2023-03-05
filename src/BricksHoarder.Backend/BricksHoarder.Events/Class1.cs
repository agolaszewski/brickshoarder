using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record CommandConsumed(string CommandName, Guid CommandCorrelationId) : IEvent;

    public record CommandConsumed<TCommand>(Guid CommandCorrelationId) : CommandConsumed(typeof(TCommand).FullName!, CommandCorrelationId) where TCommand : ICommand;
}