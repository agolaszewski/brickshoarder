using BricksHoarder.Commands.Sets;
using BricksHoarder.Common.DDD.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Domain.RebrickableSet;

namespace BricksHoarder.Domain.LegoSet
{
    public class LegoSetAggregate : AggregateRoot<RebrickableSetAggregate>
    {
    }

    public class SyncLegoSetAggregate
    {
        public class Handler : ICommandHandler<SyncSetLegoDataCommand, LegoSetAggregate>
        {
            public Task<LegoSetAggregate> HandleAsync(SyncSetLegoDataCommand command)
            {
                throw new NotImplementedException();
            }
        }
    }
}
