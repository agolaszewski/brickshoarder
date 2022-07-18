namespace BricksHoarder.Domain.Sets;

public record ImagesCollection(string? Main, string? Fallback)
{
    public Uri? Get()
    {
        var url = Main ?? Fallback;
        if (url is not null)
        {
            return new Uri(url);
        }

        return null;
    }
}