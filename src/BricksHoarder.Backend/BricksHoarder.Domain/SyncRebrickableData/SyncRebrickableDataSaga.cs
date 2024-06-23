using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Helpers;
using BricksHoarder.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Domain.SyncRebrickableData
{
    public class SyncRebrickableDataSaga : MassTransitStateMachine<SyncRebrickableDataSagaState>
    {
        private readonly ILogger<SyncRebrickableDataSaga> _logger;

        public SyncRebrickableDataSaga(ILogger<SyncRebrickableDataSaga> logger)
        {
            _logger = logger;

            InstanceState(x => x.CurrentState, SyncingState);

            Event(() => SyncSagaStarted, x => x.CorrelateById(x => x.Message.Id));
            Event(() => SyncThemesCommandConsumed, x => x.CorrelateById(x => x.CorrelationId!.Value));
            Event(() => SyncSetsCommandConsumed, x => x.CorrelateById(x => x.CorrelationId!.Value));
            Event(() => SetsReleased, x => x.CorrelateById(x => x.CorrelationId!.Value));
            Event(() => SetDetailsChanged, x => x.CorrelateById(x => x.CorrelationId!.Value));
            Event(() => SyncSetRebrickableDataCommandConsumed, x => x.CorrelateById(x => x.CorrelationId!.Value));
            Event(() => NoChangesToSets, x => x.CorrelateById(x => x.CorrelationId!.Value));

            Initially(When(SyncSagaStarted)
                .TransitionTo(SyncingState)
                .Then(_ => logger.LogDebug("SyncSagaStarted"))
                .ThenAsync(SendSyncThemesCommandAsync));

            DuringAny(When(SyncSagaStarted)
                .Then(context => logger.LogError("SyncSetsSaga is already running {id}", context.Saga.CorrelationId)));

            During(SyncingState, When(SyncThemesCommandConsumed)
                .Then(_ => logger.LogDebug("SyncThemesCommandConsumed"))
                .ThenAsync(ProcessSyncThemesCommandConsumedAsync));

            During(SyncingState, When(SyncSetsCommandConsumed)
                .Then(_ => logger.LogDebug("SyncSetsCommandConsumed"))
                .Then(ProcessSyncSetsCommandConsumed));

            During(SyncingState, When(NoChangesToSets)
                .Then(_ => logger.LogDebug("NoChangesToSets"))
                .Finalize());

            During(SyncingState, When(SetsReleased)
                .Then(_ => logger.LogDebug("SetReleased"))
                .ThenAsync(ProcessSetReleased));

            During(SyncingState, When(SetDetailsChanged)
                .Then(_ => logger.LogDebug("SetDetailsChanged"))
                .ThenAsync(ProcessSetDetailsChanged));

            During(SyncingState, When(SyncSetRebrickableDataCommandConsumed)
                .Then(_ => logger.LogDebug("SyncSetRebrickableDataCommandConsumed"))
                .ThenAsync(ProcessSetRebrickableDataCommandConsumed),

                When(SyncSetRebrickableDataCommandConsumed, x => x.Saga.AllSetsProcessed())
                .Finalize());

            Finally(x => x.Then(context => logger.LogInformation("SyncSetsSaga finished {id}", context.Saga.CorrelationId)));

            SetCompletedWhenFinalized();
        }

        public Event<NoChangesToSets> NoChangesToSets { get; }

        public Event<BatchEvent<SetDetailsChanged>> SetDetailsChanged { get; }

        public Event<BatchEvent<SetReleased>> SetsReleased { get; }

        public State SyncingState { get; }

        public Event<SyncSagaStarted> SyncSagaStarted { get; }

        public Event<CommandConsumed<SyncSetRebrickableDataCommand>> SyncSetRebrickableDataCommandConsumed { get; }

        public Event<CommandConsumed<SyncSetsCommand>> SyncSetsCommandConsumed { get; }

        public Event<CommandConsumed<SyncThemesCommand>> SyncThemesCommandConsumed { get; }

        private async Task ProcessSetDetailsChanged(BehaviorContext<SyncRebrickableDataSagaState, BatchEvent<SetDetailsChanged>> context)
        {
            foreach (var msg in context.Message.Collection)
            {
                context.Saga.AddSetToBeProcessed(msg.SetId);
            }

            if (context.Saga.AnySetIsCurrentlyProcessing())
            {
                return;
            }

            var first = context.Message.Collection.First();
            context.Saga.MarkSetAsCurrentlyProcessing(first.SetId);

            await SendAsync(context, new SyncSetRebrickableDataCommand(first.SetId));
            await SendAsync(context, new SyncSetLegoDataCommand(first.SetId));
        }

        private async Task ProcessSetRebrickableDataCommandConsumed(BehaviorContext<SyncRebrickableDataSagaState, CommandConsumed<SyncSetRebrickableDataCommand>> context)
        {
            context.Saga.FinishProcessing(context.Message.Command.SetId);

            _logger.LogDebug($"Synced {context.Saga.SetsToProcess.Count(x => x.State == ProcessingState.Finished)} of {context.Saga.SetsToProcess.Count}");

            if (context.Saga.AllSetsProcessed())
            {
                return;
            }

            if (context.Saga.AnySetIsCurrentlyProcessing())
            {
                return;
            }

            var set = context.Saga.GetNextUnprocessedSet();
            if (set == null)
            {
                return;
            }

            context.Saga.MarkSetAsCurrentlyProcessing(set.Id);

            await SendAsync(context, new SyncSetRebrickableDataCommand(set.Id));
            await SendAsync(context, new SyncSetLegoDataCommand(set.Id));
        }

        private async Task ProcessSetReleased(BehaviorContext<SyncRebrickableDataSagaState, BatchEvent<SetReleased>> context)
        {
            foreach (var msg in context.Message.Collection)
            {
                context.Saga.AddSetToBeProcessed(msg.SetId);
            }

            if (context.Saga.AnySetIsCurrentlyProcessing())
            {
                return;
            }

            var first = context.Message.Collection.First();
            context.Saga.MarkSetAsCurrentlyProcessing(first.SetId);

            await SendAsync(context, new SyncSetRebrickableDataCommand(first.SetId));
            await SendAsync(context, new SyncSetLegoDataCommand(first.SetId));
        }

        private void ProcessSyncSetsCommandConsumed(BehaviorContext<SyncRebrickableDataSagaState, CommandConsumed<SyncSetsCommand>> context)
        {
            context.Saga.SyncingSetsFinished = true;
        }

        private async Task ProcessSyncThemesCommandConsumedAsync(BehaviorContext<SyncRebrickableDataSagaState, CommandConsumed<SyncThemesCommand>> context)
        {
            await SendAsync(context, new SyncSetsCommand());
        }

        private async Task SendAsync<TCommand>(BehaviorContext<SyncRebrickableDataSagaState> context, TCommand command) where TCommand : ICommand
        {
            await context.Send(PathHelper.QueuePathUri<TCommand>(), command, x => x.CorrelationId = context.Saga.CorrelationId);
        }

        private async Task SendSyncThemesCommandAsync(BehaviorContext<SyncRebrickableDataSagaState, SyncSagaStarted> context)
        {
            await SendAsync(context, new SyncThemesCommand());
        }
    }
}