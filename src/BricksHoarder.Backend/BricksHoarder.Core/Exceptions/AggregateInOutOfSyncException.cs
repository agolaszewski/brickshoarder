using BricksHoarder.Core.Aggregates;

namespace BricksHoarder.Core.Exceptions
{
    public class AggregateInOutOfSyncException : Exception
    {
        public AggregateInOutOfSyncException()
        {
        }

        public AggregateInOutOfSyncException(IAggregateRoot aggregateRoot, Exception innerException)
            : base($"Cannot sync aggregate {aggregateRoot.Id} of type {aggregateRoot.GetType().FullName}", innerException)
        {
        }
    }
}