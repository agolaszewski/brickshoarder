using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Core.Aggregates;

namespace BricksHoarder.Domain.Sets
{
    public class SetAggregate : AggregateRoot<SetAggregate>
    , IApply<SetCreated>
    {
        public Dictionary<string, string> Names { get; set; } = new();

        public int Year { get; set; }

        public int ThemeId { get; set; }

        public int NumParts { get; set; }

        public ImagesCollection Images { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public void Create(CreateSetCommand command)
        {
            AddEvent<CreateSetCommand, SetCreated>(command);
        }

        public void Apply(SetCreated @event)
        {
            Id = @event.SetNumber;
            Names.Add("en-US", @event.Name);
            Year = @event.Year;
            ThemeId = @event.ThemeId;
            NumParts = @event.NumParts;
            Images = new ImagesCollection(null, new Uri(@event.SetImgUrl));
            LastModifiedDate = @event.LastModifiedDate;
        }
    }

    public record ImagesCollection(Uri? Main, Uri? Fallback)
    {
        public Uri? Get()
        {
            return Main ?? Fallback;
        }
    }
}