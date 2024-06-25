using BricksHoarder.DateTime;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Websites.Scrappers.Lego;
using BricksHoarder.Websites.Scrappers.Olx;

namespace BricksHoarder.Playwright.Console
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var factory = new DebuggingPageFactory();
            var runner = new LegoScrapper(factory, new CookiesFactory(), new DateTimeProvider());
            try
            {
                await runner.RunProductAsync(new LegoScrapper.LegoSetId("41744"));
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }
           
        }
    }
}