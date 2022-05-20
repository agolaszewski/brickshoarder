namespace BricksHoarder.Core.Aggregates
{
    public interface IAggregateDbStore
    {
        Task SaveAsync(IAggregateRoot aggregate);
    }
}
