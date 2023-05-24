using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record CommandConsumed<TCommand>(TCommand Command, string CommandName) : IEvent;
}