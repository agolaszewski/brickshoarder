using Microsoft.Playwright;

namespace BricksHoarder.Playwright
{
    public static class LocatorExtensions
    {
        public static async Task<string?> GetInnerTextIfVisibleAsync(this ILocator @that)
        {
            if (await @that.IsVisibleAsync())
            {
                return await @that.InnerTextAsync();
            }

            return null;
        }

        public static async Task<string?> GetTextContentIfVisibleAsync(this ILocator @that)
        {
            if (await @that.IsVisibleAsync())
            {
                return await @that.TextContentAsync();
            }

            return null;
        }
    }
}