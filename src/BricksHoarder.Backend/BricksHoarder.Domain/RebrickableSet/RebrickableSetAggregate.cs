using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Helpers;
using BricksHoarder.Events;
using Rebrickable.Api;

namespace BricksHoarder.Domain.RebrickableSet
{
    public class RebrickableSetAggregate : AggregateRoot<RebrickableSetAggregate>,
        IApply<RebrickableMinifigureAddedToSet>,
        IApply<RebrickableMinifigureDeletedFromSet>,
        IApply<RebrickableMinifigureDataSynced>,
        IApply<RebrickableSetDataSynced>
    {
        public List<Minifigure> Minifigures { get; } = new();

        public string Name { get; private set; }

        public int NumParts { get; private set; }

        public string SetImgUrl { get; private set; }

        public int ThemeId { get; private set; }

        public int Year { get; private set; }

        public void Apply(RebrickableMinifigureAddedToSet @event)
        {
            Minifigures.Add(new Minifigure(@event.MinifigureId, @event.Name, @event.Quantity, @event.ImageUrl));
        }

        public void Apply(RebrickableMinifigureDeletedFromSet @event)
        {
            Minifigures.RemoveAt(Minifigures.FindIndex(minifigure => minifigure.Id == @event.MinifigureId));
        }

        public void Apply(RebrickableMinifigureDataSynced @event)
        {
            var minifigure = Minifigures.First(minifigure => minifigure.Id == @event.MinifigureId);

            minifigure.Name = @event.Name;
            minifigure.Quantity = @event.Quantity;
            minifigure.ImageUrl = @event.ImageUrl;
        }

        public void Apply(RebrickableSetDataSynced @event)
        {
            Name = @event.Name;
            NumParts = @event.NumberOfParts;
            SetImgUrl = @event.ImageUrl;
            ThemeId = @event.ThemeId;
            Year = @event.Year;
        }

        public bool HasChanged(LegoSetsReadAsyncResponse apiSet)
        {
            return new ComparerService()
                .Compare(Name, apiSet.Name)
                .CompareStruct<int>(ThemeId, apiSet.ThemeId)
                .CompareStruct<int>(NumParts, apiSet.NumParts)
                .CompareStruct<int>(Year, apiSet.Year)
                .Compare(SetImgUrl, apiSet.SetImgUrl)
                .HasChanged;
        }

        internal void SetData(LegoSetsReadAsyncResponse apiSet)
        {
            if (HasChanged(apiSet))
            {
                AddEvent(new RebrickableSetDataSynced(Id, apiSet.Name, apiSet.ThemeId, apiSet.Year, apiSet.NumParts, apiSet.SetImgUrl));
            }
        }

        internal void SetMinifigureData(IReadOnlyList<LegoSetsMinifigsListAsyncResponse.Result> results)
        {
            var toDelete = Minifigures.Where(minifigure => results.All(x => x.Id != minifigure.Id)).ToList();
            toDelete.ForEach(minifigure => AddEvent(new RebrickableMinifigureDeletedFromSet(minifigure.Id, Id)));

            var toInsert = results.Where(minifigure => Minifigures.All(x => x.Id != minifigure.Id)).ToList();
            toInsert.ForEach(apiMinifigure => AddEvent(new RebrickableMinifigureAddedToSet(Id, apiMinifigure.Id, apiMinifigure.SetName, apiMinifigure.Quantity, apiMinifigure.SetImgUrl)));

            foreach (var minifigure in Minifigures)
            {
                var apiMinifigure = results.First(apiMinifigure => apiMinifigure.Id == minifigure.Id);
                if (minifigure.HasChanged(apiMinifigure))
                {
                    AddEvent(new RebrickableMinifigureDataSynced(Id, apiMinifigure.Id, apiMinifigure.SetName, apiMinifigure.Quantity, apiMinifigure.SetImgUrl));
                }
            }
        }
    }
}