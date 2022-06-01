namespace BricksHoarder.Abstraction.WishListService
{
    public interface IWishListService
    {
        Task<IReadOnlyList<WishListDetails>> GetAsync();

        Task AddAsync(string setNumber);
    }
}