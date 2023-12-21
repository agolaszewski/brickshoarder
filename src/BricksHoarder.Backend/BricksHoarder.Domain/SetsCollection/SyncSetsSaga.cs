using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Domain.SetsCollection
{
    public class SyncSetsSaga : MassTransitStateMachine<SyncSetsSagaState>
    {
        public SyncSetsSaga(ILogger<SyncSetsSaga> logger)
        {
            InstanceState(x => x.CurrentState, SyncingState);

            Event(() => SyncSagaStarted, x => { x.CorrelateBy(state => state.Id, context => context.Message.Id); });
            Event(() => SyncThemesCommandConsumed, x => { x.CorrelateBy(state => state.CorrelationId, context => context.CorrelationId); });
            Event(() => SyncSetsCommandConsumed, x => { x.CorrelateBy(state => state.CorrelationId, context => context.CorrelationId); });


            Initially(When(SyncSagaStarted)
                .TransitionTo(SyncingState)
                .Then(_ => logger.LogDebug("SyncSagaStarted"))
                .ThenAsync(SendSyncThemesCommandAsync));

            DuringAny(When(SyncSagaStarted)
                .Then(context => logger.LogError("SyncSetsSaga is already running {id}", context.Saga.Id)));

            During(SyncingState, When(SyncThemesCommandConsumed)
                .Then(_ => logger.LogDebug("SyncThemesCommandConsumed"))
                .Then(ProcessSyncThemesCommandConsumed));

            During(SyncingState, When(SyncSetsCommandConsumed)
                .Then(_ => logger.LogDebug("SyncSetsCommandConsumed")));

            SetCompletedWhenFinalized();
        }

        public State SyncingState { get; }

        public Event<SyncSagaStarted> SyncSagaStarted { get; }

        public Event<CommandConsumed<SyncThemesCommand>> SyncThemesCommandConsumed { get; }

        public Event<CommandConsumed<SyncSetsCommand>> SyncSetsCommandConsumed { get; }

        private void ProcessSyncThemesCommandConsumed(BehaviorContext<SyncSetsSagaState, CommandConsumed<SyncThemesCommand>> context)
        {
            context.Send(SyncSetsCommandMetadata.QueuePathUri, new SyncSetsCommand(), x => x.CorrelationId = context.Saga.CorrelationId);
        }

        private async Task SendSyncThemesCommandAsync(BehaviorContext<SyncSetsSagaState, SyncSagaStarted> action)
        {
            action.Saga.Id = action.Message.Id;
            await action.Send(SyncThemesCommandMetadata.QueuePathUri, new SyncThemesCommand(), x => x.CorrelationId = action.Saga.CorrelationId);
        }
    }
}