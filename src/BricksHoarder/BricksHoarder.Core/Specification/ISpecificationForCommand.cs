using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using FluentValidation;

namespace BricksHoarder.Core.Specification
{
    public interface ISpecificationForCommand<in TAggregate, in TCommand> where TCommand : ICommand where TAggregate : IAggregateRoot
    {
        IValidator<TAggregate> Apply(TCommand command);
    }
}
