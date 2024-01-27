using MassTransit;

namespace BricksHoarder.Domain.SyncRebrickableData;

public class SyncRebrickableDataSagaState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }

    public int CurrentState { get; set; }

    public List<ProcessingItem> SetsToProcess { get; set; } = new List<ProcessingItem>();

    public bool SyncingSetsFinished { get;  set; }
    public int Version { get; set; }

    internal void AddSetToBeProcessed(string id)
    {
        if (SetsToProcess.Any(x => x.Id == id))
        {
            return;
        }

        SetsToProcess.Add(new ProcessingItem(id, ProcessingState.NotStarted));
    }

    internal void FinishProcessing(string id)
    {
        var set = SetsToProcess.First(x => x.Id == id);
        set.State = ProcessingState.Finished;
    }

    internal bool AnySetIsCurrentlyProcessing()
    {
        return SetsToProcess.Any(x => x.State == ProcessingState.Processing);
    }

    internal bool AllSetsProcessed()
    {
        return SyncingSetsFinished && SetsToProcess.All(x => x.State == ProcessingState.Finished);
    }

    internal void MarkSetAsCurrentlyProcessing(string id)
    {
        var set = SetsToProcess.First(x => x.Id == id);
        set.State = ProcessingState.Processing;
    }

    internal string GetNextUnprocessedSet()
    {
        var set = SetsToProcess.First(x => x.State == ProcessingState.NotStarted);
        return set.Id;
    }
}