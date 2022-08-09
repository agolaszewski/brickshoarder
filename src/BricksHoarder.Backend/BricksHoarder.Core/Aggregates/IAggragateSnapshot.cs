namespace BricksHoarder.Core.Aggregates
{
    public interface IAggragateSnapshot<TAggragate> where TAggragate : IAggregateRoot
    {
        Task<TAggragate?> LoadAsync(string key);

        Task SaveAsync(string streamName, TAggragate aggregate, TimeSpan timeSpan);
    }
}