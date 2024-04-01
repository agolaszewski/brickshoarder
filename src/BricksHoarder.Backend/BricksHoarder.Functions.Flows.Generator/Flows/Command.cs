using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Functions.Flows.Generator.Generators;

namespace BricksHoarder.Functions.Flows.Generator.Flows
{
    public class Command<TCommand> : IFlowComponent where TCommand : class, ICommand
    {
        public Type Type { get; } = typeof(TCommand);
        public void Build()
        {
            CommandGenerator.Generate(Type);
            foreach (var @event in _events)
            {
                @event.Build();
            }
        }

        private readonly List<IFlowComponent> _events = new List<IFlowComponent>();

        public Command()
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