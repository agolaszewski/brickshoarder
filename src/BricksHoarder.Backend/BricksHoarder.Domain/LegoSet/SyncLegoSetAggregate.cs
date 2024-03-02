using BricksHoarder.Commands.Sets;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Websites.Scrappers.Lego;

namespace BricksHoarder.Domain.LegoSet
{
    public class SyncLegoSetData
    {
        public class Handler : ICommandHandler<SyncSetLegoDataCommand, LegoSetAggregate>
        {
            private readonly LegoScrapper _legoScrapper;
            private readonly IAggregateStore _aggregateStore;

            public Handler(IAggregateStore aggregateStore, LegoScrapper legoScrapper)
            {
                _legoScrapper = legoScrapper;
                _aggregateStore = aggregateStore;
            }

            public async Task<LegoSetAggregate> HandleAsync(SyncSetLegoDataCommand command)
            {
                var set = await _aggregateStore.GetByIdOrDefaultAsync<LegoSetAggregate>(command.SetId);
                var response = await _legoScrapper.RunAsync(command.SetId);

                set.Handle(response);

                return set;
            }
        }
    }
}