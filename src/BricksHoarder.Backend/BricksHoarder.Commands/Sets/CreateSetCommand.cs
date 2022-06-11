using BricksHoarder.Core.Commands;

public record CreateSetCommand : ICommand
{
    public string SetNumber { get; set; }

    public string Name { get; set; }

    public int Year { get; set; }

    public int ThemeId { get; set; }

    public int NumParts { get; set; }

    public string SetImgUrl { get; set; }

    public DateTime LastModifiedDate { get; set; }

    public Guid CorrelationId { get; set; }
}