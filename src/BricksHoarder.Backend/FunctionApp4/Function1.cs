using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Net;

namespace FunctionApp4
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public async Task Run([TimerTrigger("0 0 0 * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            try
            {
                _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");


                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
                var page = await browser.NewPageAsync();
                await page.GotoAsync("https://www.lego.com/pl-pl/product/batcave-shadow-box-76252");

                var text = await page.Locator("data-test=product-overview-container").Locator("data-test=product-price").InnerTextAsync();
                _logger.LogInformation(text);
            }
            catch (Exception a)
            {
                _logger.LogCritical(a.Message);
                throw;
            }
        }
    }
}