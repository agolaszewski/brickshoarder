using BricksHoarder.Frontend.Store.States;
using Fluxor;

namespace BricksHoarder.Frontend.Store.Features.WishListFeature
{
    public class WishListFeatureRegistration : Feature<WishListState>
    {
        public override string GetName() => nameof(WishListState);

        protected override WishListState GetInitialState() => new WishListState();
    }
}