using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;

namespace BricksHoarder.Common.CQRS
{
    public abstract class CommandDispatcherEventHandler<TEvent,TCommand> : IEventHandler<TEvent> where TEvent : IEvent where TCommand : class, ICommand
    {
        private readonly ICommandDispatcher _commandDispatcher;

        protected CommandDispatcherEventHandler(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        public abstract TCommand Dispatch(TEvent @event);

        public async Task HandleAsync(TEvent @event)
        {
            await _commandDispatcher.DispatchAsync(Dispatch(@event));
        }
    }
}