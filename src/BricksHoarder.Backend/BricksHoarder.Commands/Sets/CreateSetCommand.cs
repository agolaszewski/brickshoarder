using BricksHoarder.Core.Commands;

namespace BricksHoarder.Commands.Sets;

public class CreateSetCommand : ICommand
{
    public string SetNumber { get; set; }
    public string Name  { get; set; }
    public int Year { get; set; }
    public int ThemeId { get; set; }
    public int NumberOfParts { get; set; }
    public Uri? ImageUrl { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public Guid CorrelationId { get; set; }
}