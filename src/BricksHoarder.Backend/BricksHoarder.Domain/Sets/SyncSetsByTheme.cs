using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BricksHoarder.Commands.Sets;
using BricksHoarder.Commands.Themes;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Domain.Themes;
using RebrickableApi;

namespace BricksHoarder.Domain.Sets
{
    public  class SyncSetsByTheme
    {
        public class Handler : ICommandHandler<SyncSetsByThemeCommand>
        {
            private readonly IRebrickableClient _rebrickableClient;
            private readonly IAggregateStore _aggregateStore;

            public Handler(IRebrickableClient rebrickableClient, IAggregateStore aggregateStore)
            {
                _rebrickableClient = rebrickableClient;
                _aggregateStore = aggregateStore;
            }

            public async Task<IAggregateRoot> HandleAsync(SyncSetsByThemeCommand command)
            {
                var themes = _aggregateStore.GetNew<ThemesCollectionAggregate>();
                return themes;
            }
        }
    }
}
