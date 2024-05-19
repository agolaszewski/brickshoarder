namespace BricksHoarder.Websites.Scrappers.Olx
{
    public record OlxOffer
    {
        public OlxOffer(bool isFeatured, string price, bool isOlxPackage, string title, string url, bool isNew, string locationAndData)
        {
            if (price.Contains("Zamienię"))
            {
                Price = null;
            }
            else
            {
                price = price
                    .Replace("zł", string.Empty)
                    .Replace(" ", string.Empty)
                    .Replace("donegocjacji", string.Empty)
                    .Trim();

                Price = decimal.Parse(price);
            }
            

            IsFeatured = isFeatured;
            IsOlxPackage = isOlxPackage;
            Title = title;
            Url = url;
            IsNew = isNew;

            var locationAndDataParts = locationAndData.Split('-');
            Location = ParseDate(locationAndDataParts.Last());
        }

        private System.DateTime ParseDate(string dateString)
        {
            Console.WriteLine(dateString);

            if (dateString.Contains("Dzisiaj"))
            {
                return System.DateTime.Today;
            }

            dateString = dateString.Replace("Odświeżono dnia", string.Empty);

            dateString = dateString switch
            {
                string s when s.Contains("stycznia") => dateString.Replace("stycznia","01"),
                string l when l.Contains("luty") => dateString.Replace("luty", "02"),
                string m when m.Contains("marca") => dateString.Replace("marca", "03"),
                string k when k.Contains("kwietnia") => dateString.Replace("kwietnia", "04"),
                _ => throw new ArgumentOutOfRangeException(nameof(dateString), dateString, null)
            };
            dateString = dateString.Trim();
            
            return System.DateTime.ParseExact(dateString, "dd MM yyyy",null);
        }

        public bool IsNew { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public bool IsOlxPackage { get; }

        public bool IsFeatured { get; set; }

        public decimal? Price { get; }

        public System.DateTime Location { get; }
    }
}