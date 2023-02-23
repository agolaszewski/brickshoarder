using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;

namespace BricksHoarder.Domain.Sets
{
    public class CreateSet
    {
        public class Handler : ICommandHandler<CreateSetCommand>
        {
            private readonly IAggregateStore _aggregateStore;

            public Handler(IAggregateStore aggregateStore)
            {
                _aggregateStore = aggregateStore;
            }

            public Task<IAggregateRoot> HandleAsync(CreateSetCommand command)
            {
                var set = _aggregateStore.GetNew<SetAggregate>();
                set.Create(command);
                return Task.FromResult<IAggregateRoot>(set);
            }
        }
    }
}