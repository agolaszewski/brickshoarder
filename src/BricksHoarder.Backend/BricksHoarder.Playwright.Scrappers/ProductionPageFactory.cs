using Microsoft.Playwright;

namespace BricksHoarder.Playwright
{
    public class ProductionPageFactory : IPageFactory
    {
        public async Task<IPage> CreatePageAsync()
        {
            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });

            var options = new BrowserNewPageOptions();

            var page = await browser.NewPageAsync(options);
            return page;
        }
    }
}