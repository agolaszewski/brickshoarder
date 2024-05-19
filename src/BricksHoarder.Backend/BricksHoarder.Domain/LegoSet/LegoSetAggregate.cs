using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Helpers;
using BricksHoarder.Domain.RebrickableSet;
using BricksHoarder.Events;
using BricksHoarder.Websites.Scrappers.Lego;
using EnumConverterLibrary;

namespace BricksHoarder.Domain.LegoSet
{
    public class LegoSetAggregate : AggregateRoot<RebrickableSetAggregate>,
        IApply<NewLegoSetDiscovered>,
        IApply<LegoSetUpdated>
    {
        public LegoSetAggregate()
        {
        }

        public LegoSetAvailability Availability { get; private set; }

        public int? MaxQuantity { get; private set; }

        public decimal? Price { get; private set; }

        public bool? IsGift { get; private set; }

        public void Apply(NewLegoSetDiscovered @event)
        {
            Availability = @event.Availability;
            MaxQuantity = @event.MaxQuantity;
            Price = @event.Price;
            IsGift = @event.IsGift;
        }

        public void Apply(LegoSetUpdated @event)
        {
            Availability = @event.Availability;
            MaxQuantity = @event.MaxQuantity;
            Price = @event.Price;
        }

        public bool IsNewForSystem(LegoScrapperResponse response)
        {
            return Availability == LegoSetAvailability.Unknown && response.Availability != Websites.Scrappers.Lego.Availability.Unknown;
        }

        public bool HasUnknownState(LegoScrapperResponse response)
        {
            return Availability != LegoSetAvailability.Unknown && response.Availability == Websites.Scrappers.Lego.Availability.Unknown;
        }

        public bool HasChanged(LegoScrapperResponse response)
        {
            return new ComparerService()
                .Compare(Price, response.Price)
                .Compare(MaxQuantity, response.MaxQuantity)
                .Compare(Availability, response.Availability.ToAnother<LegoSetAvailability>())
                .HasChanged;
        }

        public void Update(LegoScrapperResponse response)
        {
            if (!HasChanged(response))
            {
                return;
            }

            AddEvent(new LegoSetUpdated(Id, response.Availability.ToAnother<LegoSetAvailability>(), response.MaxQuantity, response.Price));
        }

        public void CheckAvailability(LegoScrapperResponse response, System.DateTime date)
        {
            var responseAvailability = response.Availability.ToAnother<LegoSetAvailability>();

            if (Availability == responseAvailability)
            {
                return;
            }

            switch (responseAvailability)
            {
                case LegoSetAvailability.Unknown:
                    break;

                case LegoSetAvailability.Awaiting:
                    AddEvent(new LegoSetToBeReleased(Id, response.AwaitingTill!.Value));
                    break;

                case LegoSetAvailability.Available:
                    AddEvent(new LegoSetIsAvailable(Id, date));
                    break;

                case LegoSetAvailability.Pending:
                    AddEvent(new LegoSetPending(Id, response.AwaitingTill!.Value));
                    break;

                case LegoSetAvailability.RunningOut:
                    AddEvent(new LegoSetRunningOut(Id, date));
                    break;

                case LegoSetAvailability.TemporarilyUnavailable:
                    AddEvent(new LegoSetTemporarilyUnavailable(Id, date));
                    break;

                case LegoSetAvailability.Discontinued:
                    AddEvent(new LegoSetNoLongerForSale(Id, date));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(paramName: nameof(response.Availability), actualValue: response.Availability, null);
            }
        }

        public void CustomerCanBuyLess(int maxQuantity)
        {
            AddEvent(new CustomerCanBuyLessLegoSet(Id, maxQuantity));
        }

        public void CustomerCanBuyMore(int maxQuantity)
        {
            AddEvent(new CustomerCanBuyMoreLegoSet(Id, maxQuantity));
        }

        public void NewSetDiscovered(LegoScrapperResponse response)
        {
            AddEvent(new NewLegoSetDiscovered(Id, response.Name!, response.Availability.ToAnother<LegoSetAvailability>(), response.MaxQuantity, response.Price, response.ImageUrl, response.IsGift));
        }

        public void PriceDecreased(decimal price)
        {
            AddEvent(new LegoSetPriceDecreased(Id, price));
        }

        public void PriceIncreased(decimal price)
        {
            AddEvent(new LegoSetPriceIncreased(Id, price));
        }
    }
}