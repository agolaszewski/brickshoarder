using BricksHoarder.Azure.ServiceBus.Services;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Common.DDD.Exceptions;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Events;
using BricksHoarder.Websites.Scrappers.Lego;
using Polly;

namespace BricksHoarder.Domain.LegoSet
{
    public class SyncLegoSetData
    {
        public class Handler(IAggregateStore aggregateStore, LegoScrapper legoScrapper, IDateTimeProvider dateTimeProvider, IRetryCommandService retryCommandService, IIntegrationEventsQueue integrationEventsQueue) : ICommandHandler<SyncSetLegoDataCommand, LegoSetAggregate>
        {
            public async Task<LegoSetAggregate> HandleAsync(SyncSetLegoDataCommand command)
            {
                var set = await aggregateStore.GetByIdOrDefaultAsync<LegoSetAggregate>(command.SetId);
                LegoScrapperResponse response;

                if (set.IsGift.HasValue)
                {
                    response = set.IsGift.Value
                        ? await legoScrapper.RunGiftAsync(new LegoScrapper.LegoSetId(command.SetId))
                        : await legoScrapper.RunProductAsync(new LegoScrapper.LegoSetId(command.SetId));
                }
                else
                {
                    response = await Policy<LegoScrapperResponse>
                        .Handle<TimeoutException>()
                        .FallbackAsync(async _ => await legoScrapper.RunGiftAsync(new LegoScrapper.LegoSetId(command.SetId)))
                        .ExecuteAsync(() => legoScrapper.RunProductAsync(new LegoScrapper.LegoSetId(command.SetId)));
                }

                //Insert
                if (set.IsNewForSystem(response))
                {
                    set.NewSetDiscovered(response);
                }

                var now = dateTimeProvider.LocalNow(TimeZoneId.Poland);
                var tomorrow = now.AddDays(1);
                tomorrow = new System.DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 14, now.Minute, 0);

                //Update
                if (set.HasUnknownState(response))
                {
                    var retryDetails = retryCommandService.Get();
                    if (retryDetails?.RetryCount >= 7)
                    {
                        set.LegoSetNoLongerForSale(retryDetails);
                    }
                    else
                    {
                        throw new DomainException("Lego set status is unknown");
                    }
                }

                set.CheckAvailability(response, now);
                set.CheckPrice(response);
                set.CheckQuantity(response);

                set.Update(response);

                if (set.Availability is LegoSetAvailability.Awaiting or LegoSetAvailability.Pending or LegoSetAvailability.Discontinued or LegoSetAvailability.Unknown)
                {
                    return set;
                }

                integrationEventsQueue.Queue(new LegoSetInSale(set.Id, tomorrow));

                return set;
            }
        }
    }
}