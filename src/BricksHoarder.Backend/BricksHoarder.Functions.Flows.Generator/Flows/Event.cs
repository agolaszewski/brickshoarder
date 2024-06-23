using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Functions.Flows.Generator.Generators;

namespace BricksHoarder.Functions.Flows.Generator.Flows
{
    public class SagaEvent<TEvent> : IFlowComponent where TEvent : class, IEvent
    {
        private bool _isBatch;
        public Type Type { get; } = typeof(TEvent);

        public SagaEvent<TEvent> AsBatch()
        {
            _isBatch = true;
            return this;
        }

        public void Build()
        {
            if (_isBatch)
            {

            }
        }
    }

    public class Event<TEvent> : IFlowComponent where TEvent : class, IEvent
    {
        private readonly List<IFlowComponent> _commandsToSchedule = new List<IFlowComponent>();

        public Type Type { get; } = typeof(TEvent);

        public void Build()
        {
            foreach (var command in _commandsToSchedule)
            {
                EventsGenerator.ScheduleCommand(Type,command.Type);
                command.Build();
            }
        }

        public void Schedule<TCommand>() where TCommand : class, ICommand
        {
            _commandsToSchedule.Add(new Command<TCommand>());
        }
    }
}