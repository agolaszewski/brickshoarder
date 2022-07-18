using BricksHoarder.Core.Commands;

namespace BricksHoarder.Commands.Themes
{
    public record AddThemeCommand(int Id, int? ParentId, string Name, Guid CorrelationId) : ICommand;
}