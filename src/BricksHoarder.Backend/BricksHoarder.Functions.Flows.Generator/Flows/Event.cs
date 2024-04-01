using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;

namespace BricksHoarder.Functions.Flows.Generator.Flows
{
    public class Event<TEvent> : IFlowComponent where TEvent : class, IEvent
    {
        private readonly List<IFlowComponent> _commandsToSchedule = new List<IFlowComponent>();

        public Type Type { get; } = typeof(TEvent);

        public void Build()
        {
            throw new NotImplementedException();
        }

        public void Schedule<TCommand>() where TCommand : class, ICommand
        {
            _commandsToSchedule.Add(new Command<TCommand>());
        }
    }
}