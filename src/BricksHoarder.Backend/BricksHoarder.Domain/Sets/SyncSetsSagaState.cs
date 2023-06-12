using MassTransit;

namespace BricksHoarder.Domain.Sets;

public class SyncSetsSagaState : SagaStateMachineInstance
{
    public string Id { get; set; }

    public Guid CorrelationId { get; set; }

    public int CurrentState { get; set; }
}