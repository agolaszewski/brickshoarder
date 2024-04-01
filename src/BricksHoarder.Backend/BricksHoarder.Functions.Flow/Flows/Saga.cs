using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using MassTransit;

namespace BricksHoarder.Functions.Flows.Generator.Flows
{
    public class Saga<TSaga> : IFlowComponent where TSaga : class, ISaga
    {
        public Type Type { get; } = typeof(TSaga);

        private readonly List<IFlowComponent> _events = new List<IFlowComponent>();
        private readonly List<IFlowComponent> _commands = new List<IFlowComponent>();

        public void Event<TEvent>() where TEvent : class, IEvent
        {
            _events.Add(new Event<TEvent>());
        }

        public void Command<TCommand>() where TCommand : class, ICommand
        {
            _commands.Add(new Command<TCommand>());
        }
    }
}