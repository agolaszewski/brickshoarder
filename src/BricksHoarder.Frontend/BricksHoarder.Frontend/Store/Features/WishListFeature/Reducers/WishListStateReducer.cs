using BricksHoarder.Abstraction;
using BricksHoarder.Abstraction.WishListService;
using BricksHoarder.Frontend.Extensions;
using BricksHoarder.Frontend.Store.Features.WishListFeature.Actions;
using BricksHoarder.Frontend.Store.States;
using Fluxor;

namespace BricksHoarder.Frontend.Store.Features.WishListFeature.Reducers
{
    public class WishListStateReducer
    {
        [ReducerMethod]
        public static WishListState WishListDetailsLoadedReducer(WishListState state, WishListDetailsLoaded action)
        {
            return state with { WishListDetails = action.WishListDetails };
        }

        [ReducerMethod]
        public static WishListState WishListDetailsLoadedReducer(WishListState state, ItemAddedToWishList action)
        {
            return state with { WishListDetails = state.WishListDetails.Merge(new WishListDetails(action.SetNumber)) };
        }
    }
}
