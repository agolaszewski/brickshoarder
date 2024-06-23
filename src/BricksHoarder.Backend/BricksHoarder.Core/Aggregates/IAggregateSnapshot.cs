namespace BricksHoarder.Core.Aggregates
{
    public interface IAggregateSnapshot<TAggregate> where TAggregate : IAggregateRoot
    {
        Task<TAggregate?> LoadAsync(string key);

        Task SaveAsync(string streamName, TAggregate aggregate, TimeSpan timeSpan);
    }
}