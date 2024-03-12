﻿using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record NewLegoSetDiscovered(string SetId, LegoSetAvailability Availability, int? MaxQuantity, decimal? Price, string? ImageUrl) : IEvent;
}