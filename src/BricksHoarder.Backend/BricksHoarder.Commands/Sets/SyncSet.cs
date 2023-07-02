using BricksHoarder.Core.Commands;

namespace BricksHoarder.Commands.Sets
{
    public record SyncSetsByThemeCommand(int ThemeId) : ICommand;
}