using FluentValidation;
using FluentValidation.Results;

namespace BricksHoarder.Core.Exceptions
{
    public class AppValidationException : ValidationException
    {
        public AppValidationException(string aggregateId, IEnumerable<ValidationFailure> errors) : base(errors.ToList())
        {
            AggregateId = aggregateId;
        }

        public string AggregateId { get; }

        public AppValidationException(string aggregateId, string property, string message) : base(new List<ValidationFailure> { new ValidationFailure(property, message) })
        {
        }
    }
}