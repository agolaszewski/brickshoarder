using BricksHoarder.Frontend.Store.Features.WishListFeature.Actions;
using BricksHoarder.Frontend.Store.States;
using Fluxor;

namespace BricksHoarder.Frontend.Store.Features.WishListFeature.Reducers
{
    public class CalculationStateReducer
    {
        [ReducerMethod]
        public static WishListState CreditPeriodTypeChangedReducer(WishListState state, WishListDetailsLoaded action)
        {
            return state with { WishListDetails = action.WishListDetails };
        }
    }
}
