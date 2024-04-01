using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetToBeReleased(string SetId, DateTime ReleaseDate) : IEvent;

    public record LegoSetIsAvailable(string SetId, DateTime AvailableSince) : IEvent;

    public record LegoSetInSale(string SetId, DateTime NextJobRun) : IEvent;

    public record LegoSetPending(string SetId, DateTime PendingUntil) : IEvent;

    public record LegoSetRunningOut(string SetId, DateTime RunningOutSince) : IEvent;

    public record LegoSetTemporarilyUnavailable(string SetId, DateTime TemporarilyUnavailableSince) : IEvent;

   

    public record LegoSetPriceIncreased(string SetId, decimal Price) : IEvent;

    public record LegoSetPriceDecreased(string SetId, decimal Price) : IEvent;

    public record CustomerCanBuyMoreLegoSet(string SetId, decimal Price) : IEvent;

    public record CustomerCanBuyLessLegoSet(string SetId, decimal Price) : IEvent;

    public record LegoSetUpdated(string SetId, LegoSetAvailability Availability, int? MaxQuantity, decimal? Price) : IEvent;
}