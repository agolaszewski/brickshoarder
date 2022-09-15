﻿using MassTransit;

namespace BricksHoarder.Core.Events
{
    public interface IIntegrationEventsQueue
    {
        IReadOnlyCollection<IEvent> Events { get; }
        void Queue<TEvent>(TEvent @event) where TEvent : class, IEvent;
    }

    public interface IIntegrationEventDispatcher
    {
        Task DispatchAsync(ConsumeContext context);
    }
}