using BricksHoarder.Abstraction;
using BricksHoarder.Abstraction.WishListService;
using BricksHoarder.Frontend.Store.Features.WishListFeature.Actions;
using BricksHoarder.Frontend.Store.States;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;

namespace BricksHoarder.Frontend.Pages.Main
{
    public partial class Index : FluxorComponent
    {
        [Inject]
        public IState<WishListState> State { get; set; }

        [Inject]
        private IDispatcher Dispatcher { get; set; }

        [Inject]
        public IWishListService WishListService { get; set; }

        protected override void OnParametersSet()
        {
            BaseState<WishListState>.Dispatcher = Dispatcher;
        }

        protected override async Task OnInitializedAsync()
        {
            var wishList = await WishListService.GetAsync();
            Dispatcher.Dispatch(new WishListDetailsLoaded(wishList));
        }
    }
}