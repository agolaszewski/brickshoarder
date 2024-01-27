using BricksHoarder.Helpers;
using System.Globalization;

namespace BricksHoarder.Websites.Scrappers.Lego
{
    public record LegoScrapperResponse
    {
        public LegoScrapperResponse(string id, Availability availability, string? price, string? maxQuantity, System.DateTime scanDate)
        {
            var culture = new CultureInfo("pl-PL");

            Id = id;
            Availability = availability;
            Price = price.ToN<decimal>(culture);
            MaxQuantity = maxQuantity.ToN<int>(culture);
            ScanDate = scanDate;
        }

        public string Id { get; }

        public Availability Availability { get; }

        public decimal? Price { get; }

        public int? MaxQuantity { get; }

        public System.DateTime ScanDate { get; }
    }
}