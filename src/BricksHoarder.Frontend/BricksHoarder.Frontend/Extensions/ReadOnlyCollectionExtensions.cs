namespace BricksHoarder.Frontend.Extensions
{
    public static class ReadOnlyCollectionExtensions
    {
        public static IReadOnlyList<T> Merge<T>(this IReadOnlyList<T> @that, T second)
        {
            return @that.Concat(new[] { second }).ToList().AsReadOnly();
        }
    }
}
