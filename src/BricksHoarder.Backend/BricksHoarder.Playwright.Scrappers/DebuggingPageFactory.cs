using Microsoft.Playwright;

namespace BricksHoarder.Playwright;

public class DebuggingPageFactory : IPageFactory
{
    public async Task<IPage> CreatePageAsync()
    {
        Environment.SetEnvironmentVariable("PWDEBUG", "1");

        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 1000 });

        var options = new BrowserNewPageOptions();

        var page = await browser.NewPageAsync(options);
        return page;
    }
}