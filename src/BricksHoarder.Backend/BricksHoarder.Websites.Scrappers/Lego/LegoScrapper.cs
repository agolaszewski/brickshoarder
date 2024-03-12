﻿using BricksHoarder.DateTime.Noda;
using BricksHoarder.Playwright;
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

        public async Task<LegoScrapperResponse> RunAsync(string setId)
        {
            setId = setId.Split("-")[0];

            var page = await _pageFactory.CreatePageAsync();
            var cookies = await _cookiesFactory.CreateCookiesAsync("lego");
            await page.Context.AddCookiesAsync(cookies);

            await page.GotoAsync($"https://www.lego.com/pl-pl/product/{setId}");

            var notFoundPage = await page.Locator("data-test=error-link-cta").IsVisibleAsync();
            if (notFoundPage)
            {
                return new LegoScrapperResponse(setId, null, Availability.Unknown, null, null, null, _dateTimeProvider.UtcNow());
            }

            var name = await page.Locator("data-test=product-overview-name").TextContentAsync();

            await page.Locator("data-test=gallery-thumbnail-1").ClickAsync();
            var pictureSrcset = await page.Locator("picture[data-test=mediagallery-image-1] > source >> nth=0").GetAttributeAsync("srcset");
            var pictureUrl = pictureSrcset.Split(",").First().Trim().Replace("1x", string.Empty);
            var pictureBuilder = HttpUtility.ParseQueryString(pictureUrl);
            pictureBuilder.Set("quality", "90");
            pictureBuilder.Set("width", "800");
            pictureBuilder.Set("height", "800");
            pictureBuilder.Set("dpr", "2");

            var container = page.Locator("data-test=product-overview-container");
            var availabilityText = await container.Locator("data-test=product-overview-availability").TextContentAsync();

            var badges = page.Locator("div[class^=ProductOverviewstyles__Badges]");
            var productsStatus = await badges.Locator("data-test=product-flag").GetTextIfVisibleAsync();
            var isSale = await badges.Locator("data-test=sale-percentage").IsVisibleAsync();

            var availability = AvailabilityFactory.Make(availabilityText, productsStatus);

            if (availability is Availability.Unknown or Availability.Discontinued)
            {
                return new LegoScrapperResponse(setId, name, availability, null, null, pictureBuilder.ToString()!, _dateTimeProvider.UtcNow());
            }

            System.DateTime? awaitingTill = null;
            if (availability is Availability.Awaiting)
            {
                var fromWhen = availabilityText?.Replace("Dostępne od", string.Empty).Replace("Zamów ten produkt w przedsprzedaży już dziś; wysyłka rozpocznie się", string.Empty).Trim();
                awaitingTill = System.DateTime.ParseExact(fromWhen, "d MMMM yyyy", new CultureInfo("pl-Pl"));
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

            return new LegoScrapperResponse(setId, name, availability, price, maxQuantity, pictureBuilder.ToString()!, awaitingTill);
        }
    }
}