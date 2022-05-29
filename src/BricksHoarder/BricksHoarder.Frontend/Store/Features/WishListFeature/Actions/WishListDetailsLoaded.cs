using BricksHoarder.Frontend.Providers;

namespace BricksHoarder.Frontend.Store.Features.WishListFeature.Actions
{
    public record WishListDetailsLoaded(IReadOnlyList<WishListDetails> WishListDetails);
}