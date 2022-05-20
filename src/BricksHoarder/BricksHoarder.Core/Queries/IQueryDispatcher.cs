using BricksHoarder.Core.Commands;

namespace BricksHoarder.Core.Queries
{
    public interface IQueryDispatcher
    {
        Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query)
            where TQuery : class, IQuery;

        Task<TResult> DispatchAsync<TRequest, TQuery, TResult>(TRequest request)
            where TQuery : class, IQuery
            where TRequest : class, IRequest;

        Task<TResult> DispatchAsync<TRequest, TQuery, TResult>(TRequest request, Action<TQuery> afterMap)
            where TQuery : class, IQuery
            where TRequest : class, IRequest;
    }
}
