using BricksHoarder.Abstraction;
using BricksHoarder.Abstraction.WishListService;

namespace BricksHoarder.Frontend.Store.Features.WishListFeature.Actions
{
    public record WishListDetailsLoaded(IReadOnlyList<WishListDetails> WishListDetails);
}