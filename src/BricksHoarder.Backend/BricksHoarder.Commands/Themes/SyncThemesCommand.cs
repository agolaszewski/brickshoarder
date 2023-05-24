using BricksHoarder.Core.Commands;

namespace BricksHoarder.Commands.Themes
{
    public record SyncThemesCommand(string Id) : ICommand;
}