using Microsoft.Playwright;

namespace BricksHoarder.Playwright
{
    public class ProductionPageFactory : IPageFactory, IAsyncDisposable
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;

        public async Task<IPage> CreatePageAsync()
        {
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true, Timeout = 10000, ChromiumSandbox = false });

            var options = new BrowserNewPageOptions()
            {
                JavaScriptEnabled = false
            };

            var page = await _browser.NewPageAsync(options);
            await page.RouteAsync("**/*.{png,jpg,jpeg,svg,css}*", route => route.AbortAsync());
            return page;
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser != null)
            {
                await _browser.DisposeAsync();
            }

            _playwright?.Dispose();
        }
    }
}