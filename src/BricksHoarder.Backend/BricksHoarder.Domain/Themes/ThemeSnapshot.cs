namespace BricksHoarder.Domain.Themes;

public record ThemeSnapshot
{
    public ThemeSnapshot()
    {
    }

    public ThemeSnapshot(Theme theme)
    {
        Id = theme.Id;
        Name = theme.Name;
        ParentId = theme.Parent?.Id;
    }

    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? ParentId { get; set; }
}