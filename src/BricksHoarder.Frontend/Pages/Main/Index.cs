using BricksHoarder.Frontend.Providers;
using BricksHoarder.Frontend.Store.Features.WishListFeature.Actions;
using BricksHoarder.Frontend.Store.States;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using KalkulatorKredytuHipotecznego.Store.States;
using Microsoft.AspNetCore.Components;

namespace BricksHoarder.Frontend.Pages.Main
{
    public partial class Index : FluxorComponent
    {
        [Inject]
        public IState<WishListState> State { get; set; }

        [Inject]
        private IDispatcher Dispatcher { get; set; }

        protected override void OnParametersSet()
        {
            BaseState<WishListState>.Dispatcher = Dispatcher;
        }

        protected override void OnInitialized()
        {
           var provider = new WishListDataProvider();
           var wishList = provider.GetWishListDetails();
           Dispatcher.Dispatch(new WishListDetailsLoaded(wishList));
        }
    }
}
