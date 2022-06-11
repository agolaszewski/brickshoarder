namespace BricksHoarder.Domain.Sets;

public record ImagesCollection(Uri? Main, Uri? Fallback)
{
    public Uri? Get()
    {
        return Main ?? Fallback;
    }
}