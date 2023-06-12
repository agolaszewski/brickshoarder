using BricksHoarder.Commands.Themes;
using BricksHoarder.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Domain.Sets
{
    public class SyncSetsSaga : MassTransitStateMachine<SyncSetsSagaState>
    {
        public SyncSetsSaga(ILogger<SyncSetsSaga> logger)
        {
            InstanceState(x => x.CurrentState, SyncingThemesState);

            Event(() => SyncSagaStarted, x => { x.CorrelateBy(state => state.Id, context => context.Message.Id); });
            Event(() => SyncThemesCommandConsumed, x => { x.CorrelateBy(state => state.Id, context => context.Message.Command.Id); });

            Initially(When(SyncSagaStarted)
                .TransitionTo(SyncingThemesState)
                .Then(action =>
                {
                    action.Saga.Id = action.Message.Id;
                    action.Send(new Uri($"queue:{nameof(SyncThemesCommand)}"), new SyncThemesCommand(action.Saga.Id), x => x.CorrelationId = action.Saga.CorrelationId);
                }));

            During(SyncingThemesState,
                When(SyncThemesCommandConsumed)
                    .Then(action => logger.LogCritical("XD")).Finalize());

            SetCompletedWhenFinalized();
        }

        public State SyncingThemesState { get; }

        public Event<CommandConsumed<SyncThemesCommand>> SyncThemesCommandConsumed { get; }

        public Event<SyncSagaStarted> SyncSagaStarted { get; }
    }
}