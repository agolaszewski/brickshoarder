﻿namespace BricksHoarder.Websites.Scrappers.Lego
{
    internal static class AvailabilityFactory
    {
        public static Availability Make(string? text, string? productStatus)
        {
            if (productStatus == "Ostatnia szansa")
            {
                return Availability.RunningOut;
            }

            if (productStatus == "Ostatnia szansa")
            {
                return Availability.RunningOut;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return Availability.Unknown;
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
                _ => Availability.Unknown
            };
        }
    }
}