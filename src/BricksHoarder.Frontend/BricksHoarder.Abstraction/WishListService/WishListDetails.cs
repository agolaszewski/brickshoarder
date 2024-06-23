namespace BricksHoarder.Abstraction.WishListService;

public record WishListDetails
{
    public WishListDetails(string setNumber)
    {
        SetNumber = setNumber;
        LegoUrl = $"https://www.lego.com/pl-pl/product/{setNumber}";
        PromoKlockiUrl = $"https://promoklocki.pl/{setNumber}";
        BrickEconomyUrl = $"https://www.brickeconomy.com/search?query={setNumber}";
        OlxUrl = $"https://www.olx.pl/oferty/q-lego-{setNumber}/?search%5Border%5D=filter_float_price%3Aasc";
    }

    public string OlxUrl { get; set; }

    public string BrickEconomyUrl { get; set; }

    public string PromoKlockiUrl { get; set; }

    public string LegoUrl { get; set; }

    public string SetNumber { get; set; }
}