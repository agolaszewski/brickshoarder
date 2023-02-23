using BricksHoarder.Core.Commands;

namespace BricksHoarder.Core.Events
{
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        Task<IReadOnlyList<ICommand>> HandleAsync(TEvent @event);
    }
}