using BricksHoarder.Core.Commands;

namespace BricksHoarder.Commands.Themes
{
    public record SyncThemesCommand : ICommand
    {
        public Guid CorrelationId { get; init; }
    }
}