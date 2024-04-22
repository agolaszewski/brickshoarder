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

        public SagaEvent<TEvent> Event<TEvent>() where TEvent : class, IEvent
        {
            var batch = new SagaEvent<TEvent>();
            _events.Add(batch);
            return batch;
        }

        public Command<TCommand> Command<TCommand>() where TCommand : class, ICommand
        {
            var command = new Command<TCommand>();
            _commands.Add(command);
            return command;
        }

        public void Build()
        {
            foreach (var @event in _events)
            {
                @event.Build();
            }
        }
    }
}