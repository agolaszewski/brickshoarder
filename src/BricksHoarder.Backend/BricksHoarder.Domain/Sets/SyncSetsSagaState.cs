using MassTransit;

namespace BricksHoarder.Domain.Sets;

public class SyncSetsSagaState : SagaStateMachineInstance
{
    public string Id { get; set; }

    public Guid CorrelationId { get; set; }

    public int CurrentState { get; set; }

    public List<ProcessingItem> ThemesToProcess { get; set; } = new List<ProcessingItem>();

    public bool SyncingThemesFinished { get; set; }

    public bool AnyThemeProcessing()
    {
        return ThemesToProcess.Any(x => x.State == ProcessingState.Processing);
    }

    public void AddThemeToProcessing(int id)
    {
        ThemesToProcess.Add(new ProcessingItem(id, ProcessingState.NotStarted));
    }

    internal void FinishProcessingTheme(int themeId)
    {
        var theme = ThemesToProcess.First(x => x.Id == themeId);
        theme.State = ProcessingState.Finished;
    }

    public void StartThemeProcessing(int themeId)
    {
        var theme = ThemesToProcess.First(x => x.Id == themeId);
        theme.State = ProcessingState.Processing;
    }

    internal bool AllFinished()
    {
        return SyncingThemesFinished && ThemesToProcess.All(x => x.State == ProcessingState.Finished);
    }

    internal bool HasUnfinishedThemes()
    {
        return ThemesToProcess.Any(x => x.State != ProcessingState.Finished);
    }

    internal ProcessingItem GetUnprocessedTheme()
    {
        return ThemesToProcess.First(x => x.State == ProcessingState.NotStarted);
    }
}

public class ProcessingItem
{
    public ProcessingItem()
    {
    }

    public ProcessingItem(int id, ProcessingState state)
    {
        Id = id;
        State = state;
    }

    public int Id { get; set; }

    public ProcessingState State { get; set; }
}

public enum ProcessingState
{
    NotStarted = 1,
    Processing = 2,
    Finished = 3
}