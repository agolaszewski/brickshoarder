using BricksHoarder.Helpers;
using System.Globalization;

namespace BricksHoarder.Websites.Scrappers.Lego
{
    public record LegoScrapperResponse
    {
        public static LegoScrapperResponse Unknown(string id) => new(id, null, Availability.Unknown, null, null, null, null, false);

        public LegoScrapperResponse(string id, string? name, Availability availability, string? price, string? maxQuantity, string? imageUrl, System.DateTime? awaitingTill, bool isGift)
        {
            var culture = new CultureInfo("pl-PL");

            Id = id;
            Name = name;
            Availability = availability;
            Price = price.ToN<decimal>(culture);
            MaxQuantity = maxQuantity.ToN<int>(culture);
            ImageUrl = imageUrl;
            ReleaseDate = awaitingTill;
            IsGift = isGift;
        }

        public string Id { get; }

        public string? Name { get; }

        public Availability Availability { get; }

        public decimal? Price { get; }

        public int? MaxQuantity { get; }

        public string? ImageUrl { get; }

        public System.DateTime? ReleaseDate { get; }

        public bool IsGift { get; }
    }
}