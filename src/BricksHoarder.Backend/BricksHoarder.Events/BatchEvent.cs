﻿using BricksHoarder.Core.Events;

namespace BricksHoarder.Events
{
    public class BatchEvent<TEvent> : IEvent where TEvent : IEvent, IBatch
    {
        public Guid CorrelationId { get; set; }

        public IReadOnlyList<TEvent> Collection { get; set; }

        public BatchEvent()
        {
        }

        public BatchEvent(Guid correlationId, List<TEvent> messages)
        {
            Collection = messages;
            CorrelationId = correlationId;
        }
    }
}