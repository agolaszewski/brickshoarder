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
        IApply<LegoSetPriceChanged>
    {
        public LegoSetAggregate()
        {
        }

        public LegoSetAggregate(bool isImageDownloaded)
        {
            IsImageDownloaded = isImageDownloaded;
        }

        public LegoSetAvailability Availability { get; private set; }

        public int? MaxQuantity { get; private set; }

        public decimal? Price { get; private set; }

        public bool IsImageDownloaded { get; set; }

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

        public void Handle(LegoScrapperResponse response)
        {
            CheckAvailability(response.Availability);
            CheckMaxQuantity(response.MaxQuantity);
            CheckPrice(response.Price);
        }

        private void CheckAvailability(Availability availability)
        {
            var newAvailability = availability.ToAnother<LegoSetAvailability>();
            if (Availability != newAvailability)
            {
                AddEvent(new LegoSetAvailabilityChanged(Id, newAvailability, Availability));
            }
        }

        private void CheckMaxQuantity(int? maxQuantity)
        {
            if (MaxQuantity != maxQuantity)
            {
                AddEvent(new LegoSetMaxQuantityChanged(Id, maxQuantity, MaxQuantity));
            }
        }

        private void CheckPrice(decimal? price)
        {
            if (Price != price)
            {
                AddEvent(new LegoSetPriceChanged(Id, price, Price));
            }
        }
    }
}