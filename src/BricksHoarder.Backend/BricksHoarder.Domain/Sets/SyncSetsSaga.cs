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

            Event(() => ThemesSynced, x => { x.CorrelateById(x => x.Message.HerpDerp); });
            //Event(() => SyncSagaStarted, x => { x.CorrelateById(context => context.CorrelationId!.Value); });

            //Initially(When(SyncSagaStarted)
            //    .TransitionTo(ProcessingState)
            //    .Then(_ => logger.LogInformation("XD")));

            Initially(When(ThemesSynced)
                .TransitionTo(ProcessingState)
                .Then(_ => logger.LogInformation("XD"))
                .Finalize());
        }

        public State ProcessingState { get; }

        //public Event<SyncSagaStarted> SyncSagaStarted { get; }
        public Event<ThemesSynced> ThemesSynced { get; }
    }
}