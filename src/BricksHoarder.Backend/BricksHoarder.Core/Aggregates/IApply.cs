using BricksHoarder.Core.Events;

namespace BricksHoarder.Core.Aggregates
{
    public interface IApply<in TEvent> where TEvent : IEvent
    {
        void Apply(TEvent @event);
    }
}