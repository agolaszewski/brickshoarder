using BricksHoarder.DateTime;
using BricksHoarder.Playwright;

namespace BricksHoarder.Websites.Scrappers.Lego
{
    public class LegoScrapper
    {
        private readonly IPageFactory _pageFactory;
        private readonly CookiesFactory _cookiesFactory;
        private readonly IDateTimeProvider _dateTimeProvider;

        public LegoScrapper(IPageFactory pageFactory, CookiesFactory cookiesFactory, IDateTimeProvider dateTimeProvider)
        {
            _pageFactory = pageFactory;
            _cookiesFactory = cookiesFactory;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<LegoScrapperResponse> RunAsync(string id)
        {
            var page = await _pageFactory.CreatePageAsync();
            var cookies = await _cookiesFactory.CreateCookiesAsync("lego");
            await page.Context.AddCookiesAsync(cookies);

            await page.GotoAsync($"https://www.lego.com/pl-pl/product/{id}");

            var notFoundPage = await page.Locator("data-test=error-link-cta").IsVisibleAsync();
            if (notFoundPage)
            {
                return new LegoScrapperResponse(id, Availability.Discontinued, null, null, _dateTimeProvider.UtcNow());
            }

            var container = page.Locator("data-test=product-overview-container");
            var availabilityText = await container.Locator("data-test=product-overview-availability").TextContentAsync();

            var badges = page.Locator("div[class^=ProductOverviewstyles__Badges]");
            var productsStatus = await badges.Locator("data-test=product-flag").GetTextIfVisibleAsync();
            var isSale = await badges.Locator("data-test=sale-percentage").IsVisibleAsync();

            var availability = AvailabilityFactory.Make(availabilityText, productsStatus);

            if (availability is Availability.Unknown or Availability.Discontinued)
            {
                return new LegoScrapperResponse(id, availability, null, null, _dateTimeProvider.UtcNow());
            }

            var price = await container.Locator("data-test=product-price").TextContentAsync();
            price = price!.Replace("Price", string.Empty).Replace("zł", string.Empty).Trim();

            var maxQuantity = await page.Locator("div[class^=QuantitySelectorstyles__MaxQuantityWrapper] > span").InnerTextAsync();
            maxQuantity = maxQuantity.Replace("Ograniczenie", string.Empty);

            if (isSale)
            {
                var salePrice = await container.Locator("data-test=product-price-sale").TextContentAsync();
                salePrice = salePrice!.Replace("Sale Price", string.Empty).Replace("zł", string.Empty).Trim();
                price = salePrice;
            }

            return new LegoScrapperResponse(id, availability, price, maxQuantity, _dateTimeProvider.UtcNow());
        }
    }
}