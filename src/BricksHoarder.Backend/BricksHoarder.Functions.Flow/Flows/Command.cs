using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;

namespace BricksHoarder.Functions.Flows.Generator.Flows
{
    public class Command<TCommand> : IFlowComponent where TCommand : class, ICommand
    {
        public Type Type { get; } = typeof(TCommand);

        private readonly List<IFlowComponent> _events = new List<IFlowComponent>();

        public Command()
        {

        }

        public Command(string scheduleProperty)
        {

        }

        public Event<TEvent> Event<TEvent>() where TEvent : class, IEvent
        {
            var @event = new Event<TEvent>();
            _events.Add(@event);
            return @event;
        }
    }
}