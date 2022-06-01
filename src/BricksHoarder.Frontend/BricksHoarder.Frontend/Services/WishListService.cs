using Blazored.LocalStorage;
using BricksHoarder.Abstraction.WishListService;

namespace BricksHoarder.Frontend.Services
{
    public class WishListService : IWishListService
    {
        private readonly ILocalStorageService _localStorageService;
        private const string Key = "WishList";

        public WishListService(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        public async Task<IReadOnlyList<WishListDetails>> GetAsync()
        {
            var sets = await _localStorageService.GetItemAsync<List<string>>(Key) ?? Enumerable.Empty<string>();
            return sets.Select(setNumber => new WishListDetails(setNumber)).ToList();
        }

        public async Task AddAsync(string setNumber)
        {
            var sets = await _localStorageService.GetItemAsync<List<string>>(Key) ?? new List<string>();
            sets.Add(setNumber);
            await _localStorageService.SetItemAsync(Key, sets);
        }
    }
}