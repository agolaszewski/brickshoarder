using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Events;
using BricksHoarder.Websites.Scrappers.Lego;

namespace BricksHoarder.Domain.LegoSet
{
    public class SyncLegoSetData
    {
        public class Handler : ICommandHandler<SyncSetLegoDataCommand, LegoSetAggregate>
        {
            private readonly LegoScrapper _legoScrapper;
            private readonly IAggregateStore _aggregateStore;
            private readonly IDateTimeProvider _dateTimeProvider;
            private readonly IIntegrationEventsQueue _integrationEventsQueue;

            public Handler(IAggregateStore aggregateStore, LegoScrapper legoScrapper, IDateTimeProvider dateTimeProvider, IIntegrationEventsQueue integrationEventsQueue)
            {
                _legoScrapper = legoScrapper;
                _aggregateStore = aggregateStore;
                _dateTimeProvider = dateTimeProvider;
                _integrationEventsQueue = integrationEventsQueue;
            }

            public async Task<LegoSetAggregate> HandleAsync(SyncSetLegoDataCommand command)
            {
                var set = await _aggregateStore.GetByIdOrDefaultAsync<LegoSetAggregate>(command.SetId);
                var response = await _legoScrapper.RunAsync(command.SetId);

                if (set.IsNewForSystem(response))
                {
                    if (set.Availability == LegoSetAvailability.Awaiting)
                    {
                        set.WillBeReleasedLater(response.ReleaseDate!.Value);
                    }

                    return set;
                }

                if (response.Availability == Availability.Awaiting)
                {
                    set.WillBeReleasedLater(response.ReleaseDate!.Value);
                }

                if (response.Availability == Availability.Discontinued)
                {
                    set.NoLongerForSale(_dateTimeProvider.LocalNow(TimeZoneId.Poland));
                }

                set.SetAvailability(response.Availability);
                set.SetMaxQuantity(response.MaxQuantity);
                set.SetPrice(response.Price);

                if (set.Availability is LegoSetAvailability.Awaiting or LegoSetAvailability.Discontinued)
                {
                    return set;
                }

                _integrationEventsQueue.Queue(new LegoSetNotChanged(set.Id));

                return set;
            }
        }
    }
}