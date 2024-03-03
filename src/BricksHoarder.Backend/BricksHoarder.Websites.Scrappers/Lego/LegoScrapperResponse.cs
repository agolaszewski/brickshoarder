using BricksHoarder.Helpers;
using System.Globalization;

namespace BricksHoarder.Websites.Scrappers.Lego
{
    public record LegoScrapperResponse
    {
        public LegoScrapperResponse(string id, string? name, Availability availability, string? price, string? maxQuantity, string? imageUrl, System.DateTime? awaitingTill)
        {
            var culture = new CultureInfo("pl-PL");

            Id = id;
            Name = name;
            Availability = availability;
            Price = price.ToN<decimal>(culture);
            MaxQuantity = maxQuantity.ToN<int>(culture);
            ImageUrl = imageUrl;
            ReleaseDate = awaitingTill;
        }

        public string Id { get; }

        public string? Name { get; }

        public Availability Availability { get; }

        public decimal? Price { get; }

        public int? MaxQuantity { get; }

        public string? ImageUrl { get; }

        public System.DateTime? ReleaseDate { get; }
    }
}