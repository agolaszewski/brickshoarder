namespace BricksHoarder.Domain.ThemesCollection;

public record Theme(int ThemeId, string Name, Theme? Parent)
{
    public List<Theme> Children { get; set; } = new();
}