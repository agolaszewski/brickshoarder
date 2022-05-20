using FluentValidation;
using FluentValidation.Results;

namespace BricksHoarder.Core.Exceptions
{
    public class AppValidationException : ValidationException
    {
        public AppValidationException(Guid aggregateId, IEnumerable<ValidationFailure> errors) : base(errors.ToList())
        {
            AggregateId = aggregateId;
        }

        public Guid AggregateId { get; }

        public AppValidationException(Guid aggregateId, string property, string message) : base(new List<ValidationFailure> { new ValidationFailure(property, message) })
        {
        }
    }
}
