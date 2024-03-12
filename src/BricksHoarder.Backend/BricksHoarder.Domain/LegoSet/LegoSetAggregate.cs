using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Domain.RebrickableSet;
using BricksHoarder.Events;
using BricksHoarder.Websites.Scrappers.Lego;
using EnumConverterLibrary;

namespace BricksHoarder.Domain.LegoSet
{
    public class LegoSetAggregate : AggregateRoot<RebrickableSetAggregate>,
        IApply<LegoSetAvailabilityChanged>,
        IApply<LegoSetMaxQuantityChanged>,
        IApply<LegoSetPriceChanged>,
        IApply<NewLegoSetDiscovered>
    {
        public LegoSetAggregate()
        {
        }

        public LegoSetAvailability Availability { get; private set; }

        public int? MaxQuantity { get; private set; }

        public decimal? Price { get; private set; }

        public void Apply(LegoSetAvailabilityChanged @event)
        {
            Availability = @event.NewValue;
        }

        public void Apply(LegoSetMaxQuantityChanged @event)
        {
            MaxQuantity = @event.NewValue;
        }

        public void Apply(LegoSetPriceChanged @event)
        {
            Price = @event.NewValue;
        }

        public void Apply(NewLegoSetDiscovered @event)
        {
            Availability = @event.Availability;
            MaxQuantity = @event.MaxQuantity;
            Price = @event.Price;
        }

        public bool IsNewForSystem(LegoScrapperResponse response)
        {
            if (Availability == LegoSetAvailability.Unknown && response.Availability != Websites.Scrappers.Lego.Availability.Unknown)
            {
                AddEvent(new NewLegoSetDiscovered(Id, response.Name!, response.Availability.ToAnother<LegoSetAvailability>(), response.MaxQuantity, response.Price, response.ImageUrl));
                return true;
            }

            return false;
        }


        public bool SetAvailability(Availability availability)
        {
            var newAvailability = availability.ToAnother<LegoSetAvailability>();
            if (Availability == newAvailability)
            {
                return false;
            }

            AddEvent(new LegoSetAvailabilityChanged(Id, newAvailability, Availability));
            return true;
        }

        public bool SetMaxQuantity(int? maxQuantity)
        {
            if (MaxQuantity == maxQuantity)
            {
                return false;
            }

            AddEvent(new LegoSetMaxQuantityChanged(Id, maxQuantity, MaxQuantity));
            return true;
        }

        public bool SetPrice(decimal? price)
        {
            if (Price == price)
            {
                return false;
            }

            AddEvent(new LegoSetPriceChanged(Id, price, Price));
            return true;
        }

        internal void NoLongerForSale(System.DateTime stopSaleDate)
        {
            AddEvent(new LegoSetNoLongerForSale(Id, stopSaleDate));
        }

        internal void WillBeReleasedLater(System.DateTime awaitingTill)
        {
            AddEvent(new LegoSetToBeReleased(Id, awaitingTill));
        }
    }
}