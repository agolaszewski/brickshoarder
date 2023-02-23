using MassTransit;

namespace BricksHoarder.Domain.Sets;

public class SyncSetsState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }

    public int CurrentState { get; set; }
}