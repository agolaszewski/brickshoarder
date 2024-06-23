using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record NewLegoSetDiscovered(string SetId, string Name, LegoSetAvailability Availability, int? MaxQuantity, decimal? Price, string? ImageUrl, bool IsGift) : IEvent;
}