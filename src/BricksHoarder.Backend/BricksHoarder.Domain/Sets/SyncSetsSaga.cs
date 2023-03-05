using BricksHoarder.Commands.Themes;
using BricksHoarder.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Domain.Sets
{
    public class SyncSetsSaga : MassTransitStateMachine<SyncSetsState>
    {
        public SyncSetsSaga(ILogger<SyncSetsSaga> logger)
        {
            InstanceState(x => x.CurrentState, ProcessingState);

            Event(() => SyncSagaStarted, x => { x.CorrelateById(context => context.CorrelationId!.Value); });
            Event(() => SyncThemesCommandConsumed, x => { x.CorrelateById(context => context.CorrelationId!.Value); });

            Initially(When(SyncSagaStarted)
                .TransitionTo(ProcessingState)
                .Then(action => action.Send(new Uri($"queue:{nameof(SyncThemesCommand)}"), new SyncThemesCommand(), 
                    x => x.CorrelationId = action.Saga.CorrelationId)));

            During(ProcessingState,
                When(SyncThemesCommandConsumed)
                    .Then(action => logger.LogCritical("XD")).Finalize());
        }

        public State ProcessingState { get; }

        public Event<CommandConsumed<SyncThemesCommand>> SyncThemesCommandConsumed { get; }

        public Event<SyncSagaStarted> SyncSagaStarted { get; }
    }
}