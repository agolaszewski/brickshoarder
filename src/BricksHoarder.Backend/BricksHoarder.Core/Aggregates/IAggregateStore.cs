using BricksHoarder.Core.Commands;

namespace BricksHoarder.Core.Aggregates
{
    public interface IAggregateStore
    {
        Task<TAggregate> GetByIdAsync<TAggregate>(string aggregateId) where TAggregate : class, IAggregateRoot, new();

        TAggregate GetNew<TAggregate>() where TAggregate : class, IAggregateRoot, new();

        Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregateRoot;

        Task DeleteAsync<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregateRoot;

        Task<TAggregate> GetByIdOrDefaultAsync<TAggregate>(string aggregateId) where TAggregate : class, IAggregateRoot, new();

        Task<TAggregate> GetByIdOrDefaultAsync<TAggregate>() where TAggregate : class, IAggregateRoot, new();
    }
}
