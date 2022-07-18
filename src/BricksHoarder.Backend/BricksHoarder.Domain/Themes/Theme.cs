namespace BricksHoarder.Domain.Themes;

public record Theme
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public Theme? Parent { get; set; }

    public List<Theme> Children { get; set; } = new();
}