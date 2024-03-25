using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Domain.SyncRebrickableData;
using BricksHoarder.Events;
using MassTransit;

namespace BricksHoarder.Functions.Flow
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var flow = new Flow();
            var saga = flow.Saga<SyncRebrickableDataSagaState>();
            saga.Event<SyncSagaStarted>();
            saga.Event<CommandConsumed<SyncThemesCommand>>();
            saga.Event<CommandConsumed<SyncSetsCommand>>();
            saga.Event<BatchEvent<SetReleased>>();
            saga.Event<SetDetailsChanged>();
            saga.Event<CommandConsumed<SyncSetRebrickableDataCommand>>();
            saga.Event<NoChangesToSets>();

            saga.Command<SyncThemesCommand>();
            saga.Command<SyncSetsCommand>();
            saga.Command<SyncSetLegoDataCommand>();
            saga.Command<SyncSetRebrickableDataCommand>();
        }
    }

    public class Flow
    {
        private readonly List<IFlowComponent> _sagas = new List<IFlowComponent>();

        public Saga<TSaga> Saga<TSaga>() where TSaga : class, ISaga
        {
            var saga = new Saga<TSaga>();
            _sagas.Add(saga);
            return saga;
        }
    }

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

    public class Event<TEvent> : IFlowComponent where TEvent : class, IEvent
    {
        public Type Type { get; } = typeof(TEvent);
    }

    public interface IFlowComponent
    {
        public Type Type { get; }
    }

    public class Command<TCommand> : IFlowComponent where TCommand : class, ICommand
    {
        public Type Type { get; } = typeof(TCommand);
    }
}