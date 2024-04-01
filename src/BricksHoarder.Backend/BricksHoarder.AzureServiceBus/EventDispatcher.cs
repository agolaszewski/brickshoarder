using BricksHoarder.Core.Events;
using BricksHoarder.Core.Services;
using MassTransit;

namespace BricksHoarder.Azure.ServiceBus
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IGuidService _guidService;

        public EventDispatcher(IPublishEndpoint publishEndpoint, IGuidService guidService)
        {
            _publishEndpoint = publishEndpoint;
            _guidService = guidService;
        }

        public async Task<Guid> DispatchAsync<TEvent>(TEvent @event) where TEvent : class, IEvent
        {
            var correlationId = _guidService.New;
            await _publishEndpoint.Publish(@event, callback => { callback.CorrelationId = correlationId; });
            return correlationId;
        }

        public async Task<Guid> DispatchAsync<TEvent>(TEvent @event, Guid correlationId) where TEvent : class, IEvent
        {
            await _publishEndpoint.Publish(@event, callback => { callback.CorrelationId = correlationId; });
            return correlationId;
        }

        public async Task<Guid> DispatchAsync(object @event) 
        {
            var correlationId = _guidService.New;
            await _publishEndpoint.Publish(@event, callback => { callback.CorrelationId = correlationId; });
            return correlationId;
        }
    }
}