namespace BricksHoarder.Core.Events
{
    public interface IEventDispatcher
    {
        Task<Guid> DispatchAsync<TEvent>(TEvent @event) where TEvent : class, IEvent;
    }
}