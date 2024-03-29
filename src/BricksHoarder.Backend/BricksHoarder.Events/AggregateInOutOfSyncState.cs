﻿using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public record AggregateInOutOfSyncState : IEvent
    {
        public Guid CorrelationId { get; init; }

        public string AggregateId { get; set; }

        public string Type { get; set; }

        public long Version { get; set; }
    }
}