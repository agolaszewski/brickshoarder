using BricksHoarder.Core.Commands;

namespace BricksHoarder.Commands.Sets;

public record CreateSetCommand(string SetNumber, string Name, int Year, int ThemeId, int NumberOfParts, Uri? ImageUrl, DateTime LastModifiedDate) : ICommand
{
    public Guid CorrelationId { get; set; }
}