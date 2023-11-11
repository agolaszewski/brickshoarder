using BricksHoarder.Commands.Metadata;
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
            Event(() => SyncThemesCommandConsumed, x => { x.CorrelateBy(state => state.CorrelationId, context => context.CorrelationId); });

            Initially(When(SyncSagaStarted)
                .TransitionTo(SyncingThemesState)
                .ThenAsync(SendSyncThemesCommand));

            DuringAny(When(SyncSagaStarted)
                .Then(context => logger.LogError("SyncSetsSaga is already running {id}", context.Saga.Id)));

            During(SyncingThemesState, When(SyncThemesCommandConsumed)
                .Then(_ => logger.LogDebug("SyncThemesCommandConsumed"))
                .Then(ProcessSyncThemesCommandConsumed));

            SetCompletedWhenFinalized();
        }

        public State SyncingThemesState { get; }

        public Event<SyncSagaStarted> SyncSagaStarted { get; }

        public Event<CommandConsumed<SyncThemesCommand>> SyncThemesCommandConsumed { get; }

        private void ProcessSyncThemesCommandConsumed(BehaviorContext<SyncSetsSagaState, CommandConsumed<SyncThemesCommand>> obj)
        {
            throw new NotImplementedException();
        }

        private async Task SendSyncThemesCommand(BehaviorContext<SyncSetsSagaState, SyncSagaStarted> action)
        {
            action.Saga.Id = action.Message.Id;
            await action.Send(SyncThemesCommandMetadata.QueuePathUri, new SyncThemesCommand(action.Saga.Id), x => x.CorrelationId = action.Saga.CorrelationId);
        }
    }
}