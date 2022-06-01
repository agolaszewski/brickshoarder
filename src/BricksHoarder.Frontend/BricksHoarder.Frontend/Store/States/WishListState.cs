using BricksHoarder.Abstraction;
using BricksHoarder.Abstraction.WishListService;

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