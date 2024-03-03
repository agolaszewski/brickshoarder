namespace BricksHoarder.Websites.Scrappers.Lego
{
    internal static class AvailabilityFactory
    {
        public static Availability Make(string? text, string? productStatus)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Availability.Unknown;
            }

            if (productStatus == "Ostatnia szansa")
            {
                return Availability.RunningOut;
            }

            if (productStatus == "Ostatnia szansa")
            {
                return Availability.RunningOut;
            }

            if (text.Contains("zamówienia oczekujące"))
            {
                return Availability.Pending;
            }

            if (text.Contains("przedsprzedaży"))
            {
                return Availability.Awaiting;
            }

            return text switch
            {
                "Dostępne teraz" => Availability.Available,
                "Produkt wycofany – zakończono produkcję" => Availability.Discontinued,
                "Chwilowo niedostępne" => Availability.TemporarilyUnavailable,
                "Dostępne od" => Availability.Pending,
                _ => Availability.Unknown
            };
        }
    }
}