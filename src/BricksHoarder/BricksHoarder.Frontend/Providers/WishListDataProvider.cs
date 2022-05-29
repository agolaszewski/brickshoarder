namespace BricksHoarder.Frontend.Providers
{
    public class WishListDataProvider
    {
        public IReadOnlyList<WishListDetails> GetWishListDetails()
        {
            return new List<WishListDetails>()
            {
                new WishListDetails("10295")
            };
        }
    }

    public record WishListDetails(string SetNumber);
}
