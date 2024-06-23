using BricksHoarder.DateTime.Noda;
using BricksHoarder.Playwright;
using Microsoft.Playwright;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace BricksHoarder.Websites.Scrappers.Olx
{
    public class OlxScrapper
    {
        private readonly IPageFactory _pageFactory;
        private readonly CookiesFactory _cookiesFactory;
        private readonly IDateTimeProvider _dateTimeProvider;

        public OlxScrapper(IPageFactory pageFactory, CookiesFactory cookiesFactory, IDateTimeProvider dateTimeProvider)
        {
            _pageFactory = pageFactory;
            _cookiesFactory = cookiesFactory;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task HandleAsync()
        {
            IPage page = await _pageFactory.CreatePageAsync();
            var cookies = await _cookiesFactory.CreateCookiesAsync("olx");
            await page.Context.AddCookiesAsync(cookies);

            List<OlxOffer> olxOffers = new List<OlxOffer>();
            for (int pageNo = 1; pageNo <= 25; pageNo++)
            {
                Console.WriteLine("STRONA : " + pageNo);
                olxOffers.AddRange(await GetOffersAsync(page, pageNo));
            }
        }

        private async Task<List<OlxOffer>> GetOffersAsync(IPage page, int pageNo)
        {
            List<OlxOffer> olxOffers = new List<OlxOffer>();

            await page.GotoAsync($"https://www.olx.pl/dla-dzieci/zabawki/klocki/klocki-plastikowe/q-lego/?page={pageNo}&search[order]=created_at:desc");
            var offers = await page.Locator("data-testid=l-card").AllAsync();
            foreach (var offer in offers)
            {
                var isFeatured = await offer.Locator("data-testid=adCard-featured").IsVisibleAsync();
                var price = await offer.Locator("data-testid=ad-price").TextContentAsync();
                var isOlxPackage = await offer.Locator("data-testid=btr-label-text").IsVisibleAsync();

                var title = await offer.Locator("h6").TextContentAsync();
                var url = await offer.Locator("div[type=list] > a").GetAttributeAsync("href");
                var isNew = await offer.Locator("span[title=Nowe]").IsVisibleAsync();

                var locationAndData = await offer.Locator("data-testid=location-date").TextContentAsync();

                olxOffers.Add(new OlxOffer(isFeatured, price!, isOlxPackage, title!, url!, isNew, locationAndData!));
            }

            return olxOffers;
        }
    }
}