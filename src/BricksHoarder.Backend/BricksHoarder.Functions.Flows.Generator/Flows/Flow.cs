using BricksHoarder.Core.Commands;
using MassTransit;

namespace BricksHoarder.Functions.Flows.Generator.Flows
{
    public class Flow
    {
        private readonly List<IFlowComponent> _sagas = new List<IFlowComponent>();
        private readonly List<IFlowComponent> _commands = new List<IFlowComponent>();

        public Saga<TSaga> Saga<TSaga>() where TSaga : class, ISaga
        {
            var saga = new Saga<TSaga>();
            _sagas.Add(saga);
            return saga;
        }

        public Command<TCommand> Command<TCommand>() where TCommand : class, ICommand
        {
            var command = new Command<TCommand>();
            _commands.Add(command);
            return command;
        }

        internal void Build()
        {
            foreach (var saga in _sagas)
            {
                saga.Build();
            }

            foreach (var command in _commands)
            {
                command.Build();
            }
        }
    }
}