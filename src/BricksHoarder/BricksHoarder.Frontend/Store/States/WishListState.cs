using BricksHoarder.Frontend.Providers;
using KalkulatorKredytuHipotecznego.Store.States;

namespace BricksHoarder.Frontend.Store.States
{
    public record WishListState : BaseState<WishListState>
    {
        public WishListState()
        {
                
        }

        public IReadOnlyList<WishListDetails> WishListDetails { get; set; }
    }
}