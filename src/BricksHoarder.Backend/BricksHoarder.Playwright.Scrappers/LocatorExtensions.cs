﻿using Microsoft.Playwright;

namespace BricksHoarder.Playwright
{
    public static class LocatorExtensions
    {
        public static async Task<string?> GetTextIfVisibleAsync(this ILocator @that)
        {
            if (await @that.IsVisibleAsync())
            {
                return await @that.InnerTextAsync();
            }

            return null;
        }
    }
}