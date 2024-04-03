using BricksHoarder.DateTime.Noda;
using BricksHoarder.Playwright;
using Microsoft.Playwright;
using System.Diagnostics;
using System.Globalization;
using System.Web;

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

        public async Task<LegoScrapperResponse> RunGiftAsync(string setId)
        {
            setId = setId.Split("-")[0];

            IPage page = await _pageFactory.CreatePageAsync();
            var cookies = await _cookiesFactory.CreateCookiesAsync("lego");
            await page.Context.AddCookiesAsync(cookies);

            await page.GotoAsync($"https://www.lego.com/pl-pl/product/{setId}");

            var notFoundPage = await page.Locator("data-test=error-link-cta").IsVisibleAsync();
            if (notFoundPage)
            {
                return LegoScrapperResponse.Unknown(setId);
            }

            return await GetGiftAsync(page, setId);
        }

        public async Task<LegoScrapperResponse> RunProductAsync(string setId)
        {
            setId = setId.Split("-")[0];

            IPage page = await _pageFactory.CreatePageAsync();
            var cookies = await _cookiesFactory.CreateCookiesAsync("lego");
            await page.Context.AddCookiesAsync(cookies);

            await page.GotoAsync($"https://www.lego.com/pl-pl/product/{setId}");

            var notFoundPage = await page.Locator("data-test=error-link-cta").IsVisibleAsync();
            if (notFoundPage)
            {
                return LegoScrapperResponse.Unknown(setId);
            }

            return await GetProductAsync(page, setId);
        }

        private async Task<string?> GetPictureAsync(IPage page, int pictureNo)
        {
            if (pictureNo < 0)
            {
                return null;
            }

            if (!await page.Locator($"data-test=gallery-thumbnail-{pictureNo}").IsVisibleAsync())
            {
                return await GetPictureAsync(page, pictureNo - 1);
            }

            await page.Locator($"data-test=gallery-thumbnail-{pictureNo}").ClickAsync();
            var pictureSrcset = await page.Locator($"picture[data-test=mediagallery-image-{pictureNo}] > source >> nth=0").GetAttributeAsync("srcset");
            var pictureUrl = pictureSrcset!.Split(",").First().Trim().Replace("1x", string.Empty);
            var pictureBuilder = HttpUtility.ParseQueryString(pictureUrl);
            pictureBuilder.Set("quality", "90");
            pictureBuilder.Set("width", "800");
            pictureBuilder.Set("height", "800");
            pictureBuilder.Set("dpr", "2");

            return pictureBuilder.ToString()!;
        }

        private async Task<LegoScrapperResponse> GetGiftAsync(IPage page, string setId)
        {
            var name = await page.Locator("data-test=expanded-promo-card-product-title").TextContentAsync();
            var imgUrl = await GetPictureAsync(page, 1);

            var availability = await page.Locator("data-test=pdp-gwp-section").Locator("span[class^=GwpPdpSection_not-available]").IsVisibleAsync() ? Availability.Discontinued : Availability.Available;

            return new LegoScrapperResponse(setId, name, availability, null, null, imgUrl, null, true);
        }

        private async Task<LegoScrapperResponse> GetProductAsync(IPage page, string setId)
        {
            var name = await page.Locator("data-test=product-overview-name").GetTextContentIfVisibleAsync();
            var imgUrl = await GetPictureAsync(page, 1);

            var container = page.Locator("data-test=product-overview-container");
            var availabilityText = await container.Locator("data-test=product-overview-availability").GetTextContentIfVisibleAsync();
            
            var badges = page.Locator("div[class^=ProductOverviewstyles__Badges]");
            var productsStatus = await badges.Locator("data-test=product-flag")
            .Filter(new LocatorFilterOptions() { HasNotTextString = "Nowość", })
            .Filter(new LocatorFilterOptions() { HasNotTextString = "Specjalne", })
            .GetInnerTextIfVisibleAsync();

            var availability = AvailabilityFactory.Make(availabilityText, productsStatus);

            if (availability is Availability.Unknown or Availability.Discontinued)
            {
                return new LegoScrapperResponse(setId, name, availability, null, null, imgUrl, _dateTimeProvider.UtcNow(), false);
            }

            System.DateTime? awaitingTill = null;
            if (availability is Availability.Awaiting)
            {
                var fromWhen = availabilityText!.Replace("Dostępne od", string.Empty).Replace("Zamów ten produkt w przedsprzedaży już dziś; wysyłka rozpocznie się", string.Empty).Trim();
                awaitingTill = System.DateTime.ParseExact(fromWhen, "d MMMM yyyy", new CultureInfo("pl-Pl"));
            }

            if (availability is Availability.Pending)
            {
                var fromWhen = availabilityText!.Replace("Przyjmujemy zamówienia oczekujące, wysyłka rozpocznie się", string.Empty).Trim();
                awaitingTill = System.DateTime.ParseExact(fromWhen, "d MMMM yyyy", new CultureInfo("pl-Pl"));
            }

            var price = await container.Locator("data-test=product-price").GetTextContentIfVisibleAsync();
            price = price?.Replace("Price", string.Empty).Replace("zł", string.Empty).Trim();
            
            var maxQuantity = await page.Locator("div[class^=QuantitySelectorstyles__MaxQuantityWrapper] > span").GetTextContentIfVisibleAsync();
            maxQuantity = maxQuantity?.Replace("Ograniczenie", string.Empty);

            var isSale = await badges.Locator("data-test=sale-percentage").IsVisibleAsync();
            if (isSale)
            {
                var salePrice = await container.Locator("data-test=product-price-sale").TextContentAsync();
                salePrice = salePrice!.Replace("Sale Price", string.Empty).Replace("zł", string.Empty).Trim();
                price = salePrice;
            }

            return new LegoScrapperResponse(setId, name, availability, price, maxQuantity, imgUrl, awaitingTill, false);
        }
    }
}