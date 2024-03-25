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
            _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true, Timeout = 10000 });

            var options = new BrowserNewPageOptions();

            var page = await _browser.NewPageAsync(options);
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