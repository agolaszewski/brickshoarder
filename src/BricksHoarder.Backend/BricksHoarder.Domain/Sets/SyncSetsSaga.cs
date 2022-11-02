using BricksHoarder.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Domain.Sets
{
    public class SyncSetsState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }

        public int CurrentState { get; set; }
    }

    public class SyncSetsSaga : MassTransitStateMachine<SyncSetsState>
    {
        public SyncSetsSaga(ILogger<SyncSetsSaga> logger)
        {
            InstanceState(x => x.CurrentState, ProcessingState);

            Event(() => ThemesSynced, x =>
            {
                x.CorrelateById(x => x.CorrelationId!.Value);
            });

            Initially(When(ThemesSynced)
                .TransitionTo(ProcessingState)
                .Then(_ => logger.LogInformation("XD"))
                .Finalize());
        }

        public State ProcessingState { get; }

        public Event<ThemesSynced> ThemesSynced { get; }
    }
}