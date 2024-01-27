using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Commands;

namespace BricksHoarder.Domain.LegoSet
{
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