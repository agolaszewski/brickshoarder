using MassTransit;

namespace BricksHoarder.Core.Events
{
    public interface IIntegrationEventDispatcher
    {
        Task DispatchAsync(ConsumeContext context);
    }
}