using MassTransit;
using System.Diagnostics;

namespace BricksHoarder.Domain.SyncRebrickableData;

public class SyncRebrickableDataSagaState : SagaStateMachineInstance
{
    public string Id { get; set; }

    public Guid CorrelationId { get; set; }

    public int CurrentState { get; set; }

    public List<ProcessingItem> ThemesToProcess { get; set; } = new List<ProcessingItem>();

    internal void AddSetToBeProcessed(string id)
    {
        if (ThemesToProcess.Any(x => x.Id == id))
        {
            return;
        }

        ThemesToProcess.Add(new ProcessingItem(id,ProcessingState.NotStarted));
    }

    internal bool AnySetIsCurrentlyProcessing()
    {
        return ThemesToProcess.Any(x => x.State == ProcessingState.Processing);
    }

    internal void ProcessSet(string id)
    {
        throw new NotImplementedException();
    }
}