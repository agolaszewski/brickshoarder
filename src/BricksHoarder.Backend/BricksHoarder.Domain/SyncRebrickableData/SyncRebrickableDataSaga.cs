﻿using BricksHoarder.Commands.Metadata;
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

            Event(() => SyncSagaStarted, x => { x.CorrelateBy(state => state.Id, context => context.Message.Id); });
            Event(() => SyncThemesCommandConsumed, x => { x.CorrelateBy(state => state.CorrelationId, context => context.CorrelationId); });
            Event(() => SyncSetsCommandConsumed, x => { x.CorrelateBy(state => state.CorrelationId, context => context.CorrelationId); });
            Event(() => SetReleased, x => { x.CorrelateBy(state => state.CorrelationId, context => context.CorrelationId); });
            Event(() => SetDetailsChanged, x => { x.CorrelateBy(state => state.CorrelationId, context => context.CorrelationId); });
            Event(() => FetchSetRebrickableDataCommandConsumed, x => { x.CorrelateBy(state => state.CorrelationId, context => context.CorrelationId); });
            Event(() => NoChangesToSets, x => { x.CorrelateBy(state => state.CorrelationId, context => context.CorrelationId); });

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
                .Then(_ => logger.LogDebug("SyncSetsCommandConsumed"))
                .Then(ProcessSyncSetsCommandConsumed));

            During(SyncingState, When(NoChangesToSets)
                .Then(_ => logger.LogDebug("NoChangesToSets"))
                .Finalize());

            During(SyncingState, When(SetReleased)
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
                

            Finally(x =>  x.Then(context => logger.LogError("SyncSetsSaga finished {id}", context.Saga.Id)));

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

        public Event<SetReleased> SetReleased { get; }

        public Event<SetDetailsChanged> SetDetailsChanged { get; }

        public Event<CommandConsumed<SyncSetRebrickableDataCommand>> FetchSetRebrickableDataCommandConsumed { get; }

        public Event<NoChangesToSets> NoChangesToSets { get; }

        private async Task SendSyncThemesCommandAsync(BehaviorContext<SyncRebrickableDataSagaState, SyncSagaStarted> action)
        {
            action.Saga.Id = action.Message.Id;
            await action.Send(SyncThemesCommandMetadata.QueuePathUri, new SyncThemesCommand(), x => x.CorrelationId = action.Saga.CorrelationId);
        }

        private void ProcessSyncThemesCommandConsumed(BehaviorContext<SyncRebrickableDataSagaState, CommandConsumed<SyncThemesCommand>> context)
        {
            context.Send(SyncSetsCommandMetadata.QueuePathUri, new SyncSetsCommand(), x => x.CorrelationId = context.Saga.CorrelationId);
        }

        private void ProcessSetReleased(BehaviorContext<SyncRebrickableDataSagaState, SetReleased> context)
        {
            context.Saga.AddSetToBeProcessed(context.Message.Id);
            if (!context.Saga.AnySetIsCurrentlyProcessing())
            {
                context.Saga.MarkSetAsCurrentlyProcessing(context.Message.Id);
                context.Send(SyncSetRebrickableDataCommandMetadata.QueuePathUri, new SyncSetRebrickableDataCommand(context.Message.Id), x => x.CorrelationId = context.Saga.CorrelationId);
            }
        }

        private void ProcessSetDetailsChanged(BehaviorContext<SyncRebrickableDataSagaState, SetDetailsChanged> context)
        {
            context.Saga.AddSetToBeProcessed(context.Message.Id);
            if (!context.Saga.AnySetIsCurrentlyProcessing())
            {
                context.Saga.MarkSetAsCurrentlyProcessing(context.Message.Id);
                context.Send(SyncSetRebrickableDataCommandMetadata.QueuePathUri, new SyncSetRebrickableDataCommand(context.Message.Id), x => x.CorrelationId = context.Saga.CorrelationId);
            }
        }

        private void ProcessFetchSetRebrickableDataCommandConsumed(BehaviorContext<SyncRebrickableDataSagaState, CommandConsumed<SyncSetRebrickableDataCommand>> context)
        {
           context.Saga.FinishProcessing(context.Message.Command.Id);

           if (context.Saga.AllSetsProcessed())
           {
               return;
           }

           var setId = context.Saga.GetNextUnprocessedSet();
           context.Saga.MarkSetAsCurrentlyProcessing(setId);
           context.Send(SyncSetRebrickableDataCommandMetadata.QueuePathUri, new SyncSetRebrickableDataCommand(setId), x => x.CorrelationId = context.Saga.CorrelationId);
        }
    }
}