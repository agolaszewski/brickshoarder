namespace BricksHoarder.Core.Aggregates
{
    public interface IAggregateMap<in TAggregate> where TAggregate : IAggregateRoot
    {
        Task<bool> ProbeAsync(TAggregate aggregate);

        Task CreateAsync(TAggregate aggregate);

        Task UpdateAsync(TAggregate aggregate);

        Task DeleteAsync(TAggregate aggregate);
    }
}
