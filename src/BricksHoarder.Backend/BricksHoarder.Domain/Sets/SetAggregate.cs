//using BricksHoarder.Common.DDD.Aggregates;
//using BricksHoarder.Core.Aggregates;
//using BricksHoarder.Events;

//namespace BricksHoarder.Domain.Sets
//{
//    public class SetAggregate : AggregateRoot<SetAggregate>
//    , IApply<SetCreated>
//    {
//        public Dictionary<string, string> Names { get; set; } = new();

//        public int Year { get; set; }

//        public int ThemeId { get; set; }

//        public int NumberOfParts { get; set; }

//        public ImagesCollection Images { get; set; }

//        public DateTime LastModifiedDate { get; set; }

//        public void Apply(SetCreated @event)
//        {
//            Id = @event.SetNumber;
//            Names.Add("en-US", @event.Name);
//            Year = @event.Year;
//            ThemeId = @event.ThemeId;
//            NumberOfParts = @event.NumberOfParts;
//            Images = new ImagesCollection(null, @event.ImageUrl?.OriginalString);
//            LastModifiedDate = @event.LastModifiedDate;
//        }
//    }
//}