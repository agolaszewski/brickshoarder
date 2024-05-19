using BricksHoarder.Core.Helpers;
using Rebrickable.Api;

namespace BricksHoarder.Domain.RebrickableSet
{
    public record Minifigure(int Id, string Name, int Quantity, string ImageUrl)
    {
        public bool HasChanged(LegoSetsMinifigsListAsyncResponse.Result apiMinifigures)
        {
            return new ComparerService()
                .Compare(Name, apiMinifigures.SetName)
                .CompareStruct<int>(Quantity, apiMinifigures.Quantity)
                .Compare(ImageUrl, apiMinifigures.SetImgUrl)
                .HasChanged;
        }
    }
}