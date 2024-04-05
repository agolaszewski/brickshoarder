using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Core.Services;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Events;
using BricksHoarder.Websites.Scrappers.Lego;
using Polly;

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
            private readonly IRandomService _randomService;

            public Handler(IAggregateStore aggregateStore, LegoScrapper legoScrapper, IDateTimeProvider dateTimeProvider, IRandomService randomService, IIntegrationEventsQueue integrationEventsQueue)
            {
                _legoScrapper = legoScrapper;
                _aggregateStore = aggregateStore;
                _dateTimeProvider = dateTimeProvider;
                _integrationEventsQueue = integrationEventsQueue;
                _randomService = randomService;
            }

            public async Task<LegoSetAggregate> HandleAsync(SyncSetLegoDataCommand command)
            {
                var set = await _aggregateStore.GetByIdOrDefaultAsync<LegoSetAggregate>(command.SetId);
                LegoScrapperResponse response;

                if (set.IsGift.HasValue)
                {
                    response = set.IsGift.Value
                        ? await _legoScrapper.RunGiftAsync(command.SetId)
                        : await _legoScrapper.RunProductAsync(command.SetId);
                }
                else
                {
                    response = await Policy<LegoScrapperResponse>
                        .Handle<TimeoutException>()
                        .FallbackAsync(async _ => await _legoScrapper.RunGiftAsync(command.SetId))
                        .ExecuteAsync(() => _legoScrapper.RunProductAsync(command.SetId));
                }

                //Insert
                if (set.IsNewForSystem(response))
                {
                    set.NewSetDiscovered(response);
                }

                //Update
                if (set.HasUnknownState(response))
                {
                    throw new InvalidOperationException($"Scrapper returned Unknown state for set {set.Id}");
                }

                var now = _dateTimeProvider.LocalNow(TimeZoneId.Poland);
                set.CheckAvailability(response, now.Date);

                if (response.Price > set.Price)
                {
                    set.PriceIncreased(response.Price.Value);
                }

                if (response.Price < set.Price)
                {
                    set.PriceDecreased(response.Price.Value);
                }

                if (response.MaxQuantity > set.MaxQuantity)
                {
                    set.CustomerCanBuyMore(response.MaxQuantity.Value);
                }

                if (response.MaxQuantity < set.MaxQuantity)
                {
                    set.CustomerCanBuyLess(response.MaxQuantity.Value);
                }

                set.Update(response);

                if (set.Availability is LegoSetAvailability.Awaiting or LegoSetAvailability.Pending or LegoSetAvailability.Discontinued or LegoSetAvailability.Unknown)
                {
                    return set;
                }

                var start = now.AddDays(1).Date.AddHours(8).ToUniversalTime();
                var end = now.AddDays(1).Date.AddHours(12).ToUniversalTime();
                
                _integrationEventsQueue.Queue(new LegoSetInSale(set.Id, _randomService.Between(start, end)));

                return set;
            }
        }
    }
}