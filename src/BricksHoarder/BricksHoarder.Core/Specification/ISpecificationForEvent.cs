using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Events;
using FluentValidation;

namespace BricksHoarder.Core.Specification
{
    public interface ISpecificationForEvent<in TAggregate, in TEvent> where TEvent : IEvent where TAggregate : IAggregateRoot
    {
        IValidator<TAggregate> Apply(TEvent @event);
    }
}
