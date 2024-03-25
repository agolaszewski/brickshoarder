using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Domain.SyncRebrickableData
{
    public class SyncRebrickableDataSaga : MassTransitStateMachine<SyncRebrickableDataSagaState>
    {
        public SyncRebrickableDataSaga(ILogger<SyncRebrickableDataSaga> logger)
        {
            InstanceState(x => x.CurrentState, SyncingState);

            Event(() => SyncSagaStarted, x => x.CorrelateById(x => x.Message.Id));
            Event(() => SyncThemesCommandConsumed, x => x.CorrelateById(x => x.CorrelationId!.Value));
            Event(() => SyncSetsCommandConsumed, x => x.CorrelateById(x => x.CorrelationId!.Value));
            Event(() => SetsReleased, x => x.CorrelateById(x => x.CorrelationId!.Value));
            Event(() => SetDetailsChanged, x => x.CorrelateById(x => x.CorrelationId!.Value));
            Event(() => FetchSetRebrickableDataCommandConsumed, x => x.CorrelateById(x => x.CorrelationId!.Value));
            Event(() => NoChangesToSets, x => x.CorrelateById(x => x.CorrelationId!.Value));

            Initially(When(SyncSagaStarted)
                .TransitionTo(SyncingState)
                .Then(_ => logger.LogDebug("SyncSagaStarted"))
                .ThenAsync(SendSyncThemesCommandAsync));

            DuringAny(When(SyncSagaStarted)
                .Then(context => logger.LogError("SyncSetsSaga is already running {id}", context.Saga.CorrelationId)));

            During(SyncingState, When(SyncThemesCommandConsumed)
                .Then(_ => logger.LogDebug("SyncThemesCommandConsumed"))
                .Then(ProcessSyncThemesCommandConsumed));

            During(SyncingState, When(SyncSetsCommandConsumed)
                .Then(_ => logger.LogDebug("SyncSetsCommandConsumed"))
                .Then(ProcessSyncSetsCommandConsumed));

            During(SyncingState, When(NoChangesToSets)
                .Then(_ => logger.LogDebug("NoChangesToSets"))
                .Finalize());

            During(SyncingState, When(SetsReleased)
                .Then(_ => logger.LogDebug("SetReleased"))
                .Then(ProcessSetReleased));

            During(SyncingState, When(SetDetailsChanged)
                .Then(_ => logger.LogDebug("SetDetailsChanged"))
                .Then(ProcessSetDetailsChanged));

            During(SyncingState, When(FetchSetRebrickableDataCommandConsumed)
                .Then(_ => logger.LogDebug("FetchSetRebrickableDataCommandConsumed"))
                .Then(ProcessFetchSetRebrickableDataCommandConsumed),

                When(FetchSetRebrickableDataCommandConsumed, x => x.Saga.AllSetsProcessed())
                .Finalize());

            Finally(x => x.Then(context => logger.LogError("SyncSetsSaga finished {id}", context.Saga.CorrelationId)));

            SetCompletedWhenFinalized();
        }

        private void ProcessSyncSetsCommandConsumed(BehaviorContext<SyncRebrickableDataSagaState, CommandConsumed<SyncSetsCommand>> context)
        {
            context.Saga.SyncingSetsFinished = true;
        }

        public State SyncingState { get; }

        public Event<SyncSagaStarted> SyncSagaStarted { get; }

        public Event<CommandConsumed<SyncThemesCommand>> SyncThemesCommandConsumed { get; }

        public Event<CommandConsumed<SyncSetsCommand>> SyncSetsCommandConsumed { get; }

        public Event<BatchEvent<SetReleased>> SetsReleased { get; }

        public Event<SetDetailsChanged> SetDetailsChanged { get; }

        public Event<CommandConsumed<SyncSetRebrickableDataCommand>> FetchSetRebrickableDataCommandConsumed { get; }

        public Event<NoChangesToSets> NoChangesToSets { get; }

        private async Task SendSyncThemesCommandAsync(BehaviorContext<SyncRebrickableDataSagaState, SyncSagaStarted> action)
        {
            await action.Send(SyncThemesCommandMetadata.QueuePathUri, new SyncThemesCommand(), x => x.CorrelationId = action.Saga.CorrelationId);
        }

        private void ProcessSyncThemesCommandConsumed(BehaviorContext<SyncRebrickableDataSagaState, CommandConsumed<SyncThemesCommand>> context)
        {
            context.Send(SyncSetsCommandMetadata.QueuePathUri, new SyncSetsCommand(), x => x.CorrelationId = context.Saga.CorrelationId);
        }

        private void ProcessSetReleased(BehaviorContext<SyncRebrickableDataSagaState, BatchEvent<SetReleased>> context)
        {
            foreach (var msg in context.Message.Collection)
            {
                context.Saga.AddSetToBeProcessed(msg.SetId);
                context.Send(SyncSetLegoDataCommandMetadata.QueuePathUri, new SyncSetLegoDataCommand(msg.SetId), x => x.CorrelationId = context.Saga.CorrelationId);
            }

            if (!context.Saga.AnySetIsCurrentlyProcessing())
            {
                var first = context.Message.Collection.First();

                context.Saga.MarkSetAsCurrentlyProcessing(first.SetId);
                context.Send(SyncSetRebrickableDataCommandMetadata.QueuePathUri, new SyncSetRebrickableDataCommand(first.SetId), x => x.CorrelationId = context.Saga.CorrelationId);
            }
        }

        private void ProcessSetDetailsChanged(BehaviorContext<SyncRebrickableDataSagaState, SetDetailsChanged> context)
        {
            context.Saga.AddSetToBeProcessed(context.Message.SetId);
            if (!context.Saga.AnySetIsCurrentlyProcessing())
            {
                context.Saga.MarkSetAsCurrentlyProcessing(context.Message.SetId);
                context.Send(SyncSetRebrickableDataCommandMetadata.QueuePathUri, new SyncSetRebrickableDataCommand(context.Message.SetId), x => x.CorrelationId = context.Saga.CorrelationId);
            }
        }

        private void ProcessFetchSetRebrickableDataCommandConsumed(BehaviorContext<SyncRebrickableDataSagaState, CommandConsumed<SyncSetRebrickableDataCommand>> context)
        {
            context.Saga.FinishProcessing(context.Message.Command.SetId);

            if (context.Saga.AllSetsProcessed())
            {
                return;
            }

            var set = context.Saga.GetNextUnprocessedSet();
            if (set == null)
            {
                return;
            }

            context.Saga.MarkSetAsCurrentlyProcessing(set.Id);
            context.Send(SyncSetRebrickableDataCommandMetadata.QueuePathUri, new SyncSetRebrickableDataCommand(set.Id), x => x.CorrelationId = context.Saga.CorrelationId);
        }
    }
}