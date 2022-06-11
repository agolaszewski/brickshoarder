using BricksHoarder.Core.Commands;

namespace BricksHoarder.Core.Aggregates
{
    public interface IAggregateStore
    {
        Task<TAggregate> GetByIdAsync<TAggregate>(string aggregateId) where TAggregate : class, IAggregateRoot, new();

        Task<TAggregate> GetByIdAsync<TAggregate, TCommand>(string aggregateId, TCommand command)
            where TAggregate : class, IAggregateRoot, new() where TCommand : ICommand;

        TAggregate GetNew<TAggregate>() where TAggregate : class, IAggregateRoot, new();

        Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregateRoot;

        Task DeleteAsync<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregateRoot;

        Task<TAggregate> GetByIdOrDefaultAsync<TAggregate>(string aggregateId) where TAggregate : class, IAggregateRoot, new();
    }
}
