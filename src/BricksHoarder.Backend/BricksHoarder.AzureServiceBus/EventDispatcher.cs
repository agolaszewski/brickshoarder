using BricksHoarder.Core.Events;
using BricksHoarder.Core.Services;
using MassTransit;

namespace BricksHoarder.Azure.ServiceBus
{
    public class EventDispatcher(IPublishEndpoint publishEndpoint, IGuidService guidService) : IEventDispatcher
    {
        public async Task<Guid> DispatchAsync<TEvent>(TEvent @event) where TEvent : class, IEvent
        {
            var correlationId = guidService.New;
            await publishEndpoint.Publish(@event, callback => { callback.CorrelationId = correlationId; });
            return correlationId;
        }

        public async Task<Guid> DispatchAsync<TEvent>(TEvent @event, Guid correlationId) where TEvent : class, IEvent
        {
            await publishEndpoint.Publish(@event, callback => { callback.CorrelationId = correlationId; });
            return correlationId;
        }

        public async Task<Guid> DispatchAsync(object @event) 
        {
            var correlationId = guidService.New;
            await publishEndpoint.Publish(@event, callback => { callback.CorrelationId = correlationId; });
            return correlationId;
        }
    }
}