using BricksHoarder.Commands.Sets;
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
            Event(() => ThemeAdded, x => { x.CorrelateBy(state => state.CorrelationId, context => context.CorrelationId); });
            Event(() => SyncThemesCommandConsumed, x => { x.CorrelateBy(state => state.CorrelationId, context => context.CorrelationId); });
            Event(() => SyncSetsByThemeCommandConsumed, x => { x.CorrelateBy(state => state.CorrelationId, context => context.CorrelationId); });


            Initially(When(SyncSagaStarted)
                .TransitionTo(SyncingThemesState)
                .Then(SendSyncThemesCommand));

            DuringAny(When(SyncSagaStarted)
                .Then(context => logger.LogError("SyncSetsSaga is already running {id}", context.Saga.Id)));

            During(SyncingThemesState, When(ThemeAdded)
                .Then(context => logger.LogDebug("ThemeAdded {id}", context.Message.Id))
                .Then(ProcessTheme));

            During(SyncingThemesState, When(SyncThemesCommandConsumed)
                .Then(_ => logger.LogDebug("SyncThemesCommandConsumed"))
                .Then(ProcessSyncThemesCommandConsumed));

            During(SyncingThemesState, When(SyncSetsByThemeCommandConsumed)
                .Then(_ => logger.LogDebug("SyncSetsByThemeCommandConsumed"))
                .Then(ProcessSyncSetsByThemeCommandConsumed));

            SetCompletedWhenFinalized();
        }

        public State SyncingThemesState { get; }
        public Event<SyncSagaStarted> SyncSagaStarted { get; }
        public Event<ThemeAdded> ThemeAdded { get; }
        public Event<CommandConsumed<SyncThemesCommand>> SyncThemesCommandConsumed { get; }

        public Event<CommandConsumed<SyncSetsByThemeCommand>> SyncSetsByThemeCommandConsumed { get; }

        private void ProcessSyncSetsByThemeCommandConsumed(BehaviorContext<SyncSetsSagaState, CommandConsumed<SyncSetsByThemeCommand>> context)
        {
            context.Saga.FinishProcessingTheme(context.Message.Command.ThemeId);
            if (context.Saga.HasUnfinishedThemes())
            {
                var theme = context.Saga.GetUnprocessedTheme();
                context.Send(SyncSetsByThemeCommandMetadata.QueuePathUri, new SyncSetsByThemeCommand(theme.Id), x => x.CorrelationId = context.Saga.CorrelationId);
            }
        }

        private void ProcessSyncThemesCommandConsumed(BehaviorContext<SyncSetsSagaState, CommandConsumed<SyncThemesCommand>> context)
        {
            context.Saga.SyncingThemesFinished = true;
        }

        private void ProcessTheme(BehaviorContext<SyncSetsSagaState, ThemeAdded> context)
        {
            context.Saga.AddThemeToProcessing(context.Message.Id);
            if (context.Saga.AnyThemeProcessing())
            {
                return;
            }
            context.Saga.StartThemeProcessing(context.Message.Id);
            context.Send(SyncSetsByThemeCommandMetadata.QueuePathUri, new SyncSetsByThemeCommand(context.Message.Id), x => x.CorrelationId = context.Saga.CorrelationId);
        }

        private void SendSyncThemesCommand(BehaviorContext<SyncSetsSagaState, SyncSagaStarted> action)
        {
            action.Saga.Id = action.Message.Id;
            action.Send(SyncThemesCommandMetadata.QueuePathUri, new SyncThemesCommand(action.Saga.Id), x => x.CorrelationId = action.Saga.CorrelationId);
        }
    }
}