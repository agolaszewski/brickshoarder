using BricksHoarder.Core.Commands;

namespace BricksHoarder.Commands.Themes
{
    public class SyncThemesCommand : ICommand
    {
        public Guid CorrelationId { get; init; }
    }
}