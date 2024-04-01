using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;

namespace BricksHoarder.Functions.Flows.Generator.Flows
{
    public class Event<TEvent> : IFlowComponent where TEvent : class, IEvent
    {
        private readonly List<IFlowComponent> _commands = new List<IFlowComponent>();

        public Type Type { get; } = typeof(TEvent);

        public void Command<TCommand>(string scheduleProperty) where TCommand : class, ICommand
        {
            _commands.Add(new Command<TCommand>(scheduleProperty));
        }
    }
}