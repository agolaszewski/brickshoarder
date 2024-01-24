﻿using BricksHoarder.DateTime;
using BricksHoarder.Websites.Scrappers;
using BricksHoarder.Websites.Scrappers.Lego;

namespace BricksHoarder.Playwright.Console
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var factory = new DebuggingPageFactory();
            var runner = new LegoScrapper(factory, new CookiesFactory(), new DateTimeProvider());

            await runner.RunAsync("42083");
        }
    }
}