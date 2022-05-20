namespace BricksHoarder.Core.Aggregates
{
    public interface IAggregateRoot : IBaseAggregateRoot
    {
        IServiceProvider Context { get; set; }

        Task CommitAsync(IAggregateStore aggregateStore);
    }
}
