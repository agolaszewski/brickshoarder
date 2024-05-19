using BricksHoarder.Core.Helpers;
using Rebrickable.Api;

namespace BricksHoarder.Domain.RebrickableSet
{
    public class Minifigure(int id, string name, int quantity, string imageUrl)
    {
        public int Id = id;
        public string Name = name;
        public int Quantity = quantity;
        public string ImageUrl = imageUrl;

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