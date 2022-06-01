using BricksHoarder.Core.Exceptions;

namespace BricksHoarder.Core.Events
{
    public class ValidationExceptionRaised : IEvent
    {
        public ValidationExceptionRaised()
        {
        }

        public ValidationExceptionRaised(AppValidationException exception, Guid correlationId)
        {
            AggregateId = exception.AggregateId;
            Errors = exception.Errors.Select(e => e.ToString());
            CorrelationId = correlationId;
        }

        public Guid AggregateId { get; set; }

        public IEnumerable<string> Errors { get; set; } = new List<string>();

        public Guid CorrelationId { get; set; }
    }
}
