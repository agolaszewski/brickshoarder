using Microsoft.Playwright;
using System.Text.Json;

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

public class CookiesFactory
{
    public async Task<IReadOnlyList<Cookie>> CreateCookiesAsync(string page)
    {
        var content = await File.ReadAllTextAsync($"Cookies/{page}.json");
        var cookies = JsonSerializer.Deserialize<List<Cookie>>(content)!.ToList();
        return cookies;
    }
}