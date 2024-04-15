using BricksHoarder.Commands.Metadata;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record LegoSetToBeReleased(string SetId, DateTime ReleaseDate) : IEvent, IScheduling<SyncSetLegoDataCommand>
    {
        public SchedulingDetails<SyncSetLegoDataCommand> SchedulingDetails()
        {
            return new SchedulingDetails<SyncSetLegoDataCommand>($"SyncSetLegoDataCommand-{SetId}", new SyncSetLegoDataCommand(SetId), SyncSetLegoDataCommandMetadata.QueuePathUri, ReleaseDate);
        }
    }

    public record LegoSetIsAvailable(string SetId, DateTime AvailableSince) : IEvent;

    public record LegoSetInSale(string SetId, DateTime NextJobRun) : IEvent, IScheduling<SyncSetLegoDataCommand>
    {
        public SchedulingDetails<SyncSetLegoDataCommand> SchedulingDetails()
        {
            return new SchedulingDetails<SyncSetLegoDataCommand>($"SyncSetLegoDataCommand-{SetId}",new SyncSetLegoDataCommand(SetId), SyncSetLegoDataCommandMetadata.QueuePathUri, NextJobRun);
        }
    }

    public record LegoSetPending(string SetId, DateTime PendingUntil) : IEvent, IScheduling<SyncSetLegoDataCommand>
    {
        public SchedulingDetails<SyncSetLegoDataCommand> SchedulingDetails()
        {
            return new SchedulingDetails<SyncSetLegoDataCommand>($"SyncSetLegoDataCommand-{SetId}", new SyncSetLegoDataCommand(SetId), SyncSetLegoDataCommandMetadata.QueuePathUri, PendingUntil);
        }
    }

    public record LegoSetRunningOut(string SetId, DateTime RunningOutSince) : IEvent;

    public record LegoSetTemporarilyUnavailable(string SetId, DateTime TemporarilyUnavailableSince) : IEvent;

   

    public record LegoSetPriceIncreased(string SetId, decimal Price) : IEvent;

    public record LegoSetPriceDecreased(string SetId, decimal Price) : IEvent;

    public record CustomerCanBuyMoreLegoSet(string SetId, decimal Price) : IEvent;

    public record CustomerCanBuyLessLegoSet(string SetId, decimal Price) : IEvent;

    public record LegoSetUpdated(string SetId, LegoSetAvailability Availability, int? MaxQuantity, decimal? Price) : IEvent;
}