using BricksHoarder.Commands.Sets;
using BricksHoarder.Common.DDD.Exceptions;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Core.Services;
using BricksHoarder.DateTime.Noda;
using BricksHoarder.Events;
using BricksHoarder.Websites.Scrappers.Lego;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly;

namespace BricksHoarder.Domain.LegoSet
{
    public class SyncLegoSetData
    {
        public class Handler(IMessageLockService messageLockService, IAggregateStore aggregateStore, LegoScrapper legoScrapper, IDateTimeProvider dateTimeProvider, ConsumeContext consumeContext, IIntegrationEventsQueue integrationEventsQueue, ILogger<Handler> logger) : ICommandHandler<SyncSetLegoDataCommand, LegoSetAggregate>
        {
            public async Task<LegoSetAggregate> HandleAsync(SyncSetLegoDataCommand command)
            {
                var legoId = new LegoScrapper.LegoSetId(command.SetId);

                if (messageLockService.Lock($"SyncSetLegoDataCommand:{legoId.Value}", consumeContext.MessageId!.Value, dateTimeProvider.UtcNow().Date.AddDays(1)))
                {
                    logger.LogInformation($"SyncSetLegoDataCommand:{legoId.Value} was already scanned today");
                }

                var set = await aggregateStore.GetByIdOrDefaultAsync<LegoSetAggregate>(legoId.Value);
                LegoScrapperResponse response;

                if (set.IsGift.HasValue)
                {
                    response = set.IsGift.Value
                        ? await legoScrapper.RunGiftAsync(legoId)
                        : await legoScrapper.RunProductAsync(legoId);
                }
                else
                {
                    response = await Policy<LegoScrapperResponse>
                        .Handle<TimeoutException>()
                        .FallbackAsync(async _ => await legoScrapper.RunGiftAsync(legoId))
                        .ExecuteAsync(() => legoScrapper.RunProductAsync(legoId));
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
                    if (consumeContext.GetRedeliveryCount() >= 6)
                    {
                        set.LegoSetNoLongerForSale(now);
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