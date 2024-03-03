using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetAvailabilityChanged(string SetId, LegoSetAvailability NewValue, LegoSetAvailability OldValue) : IEvent;

    public record LegoSetMaxQuantityChanged(string SetId, int? NewValue, int? OldValue) : IEvent;

    public record NewLegoSetDiscovered(string SetId, LegoSetAvailability Availability, int? MaxQuantity, decimal? Price, string? ImageUrl) : IEvent;

    public record LegoSetToBeReleased(string SetId, DateTime ReleaseDate) : IEvent;

    public record LegoSetNoLongerForSale(string SetId, DateTime Dis) : IEvent;

    public record LegoSetPriceChanged(string SetId, decimal? NewValue, decimal? OldValue) : IEvent;

    public enum LegoSetAvailability
    {
        Unknown = 0,
        Awaiting = 1,
        Available = 2,
        Pending = 3,
        RunningOut = 4,
        TemporarilyUnavailable = 5,
        Discontinued = 6,
    }
}