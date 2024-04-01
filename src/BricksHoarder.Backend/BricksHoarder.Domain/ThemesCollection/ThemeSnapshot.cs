namespace BricksHoarder.Domain.ThemesCollection;

public record ThemeSnapshot
{
    public ThemeSnapshot()
    {
    }

    public ThemeSnapshot(Theme theme)
    {
        ThemeId = theme.ThemeId;
        Name = theme.Name;
        ParentId = theme.Parent?.ThemeId;
    }

    public int ThemeId { get; set; }

    public string Name { get; set; } = null!;

    public int? ParentId { get; set; }
}