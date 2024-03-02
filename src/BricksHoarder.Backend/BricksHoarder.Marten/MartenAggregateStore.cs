using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Exceptions;
using BricksHoarder.Core.Services;
using BricksHoarder.Events;
using Marten;
using Marten.Events;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace BricksHoarder.Marten
{
    public class MartenAggregateStore : IAggregateStore
    {
        private readonly ICacheService _cache;
        private readonly IServiceProvider _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IDocumentStore _eventStore;

        public MartenAggregateStore(
            IDocumentStore eventStore,
            ICacheService cache,
            IServiceProvider context,
            IPublishEndpoint publishEndpoint)
        {
            _eventStore = eventStore;
            _cache = cache;
            _context = context;
            _publishEndpoint = null;
        }

        public async Task<TAggregate> GetByIdAsync<TAggregate>(string aggregateId)
            where TAggregate : class, IAggregateRoot, new()
        {
            var aggregate = await GetByIdOrDefaultAsync<TAggregate>(aggregateId, 0);
            if (aggregate is null)
            {
                throw new AppValidationException(aggregateId, $"{typeof(TAggregate).Name}Id", $"Aggregate with id : {aggregateId} not found");
            }

            return aggregate;
        }

        public TAggregate GetNew<TAggregate>() where TAggregate : class, IAggregateRoot, new()
        {
            var model = new TAggregate { Context = _context, Version = 0 };
            return model;
        }

        public async Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregateRoot
        {
            string streamName = $"{aggregate.GetType().Name}:{aggregate.Id}";
            
            PolicyResult eventStoreOutcome = PolicyResult.Successful(null);

            if (aggregate.Events.Any())
            {
                eventStoreOutcome = await Policies.EventStoreRetryPolicy.ExecuteAndCaptureAsync(async () =>
                {
                    await using var session = _eventStore.LightweightSession();
                    session.Events.Append(streamName, aggregate.Version + aggregate.Events.Count(), aggregate.Events.Select(a => a.Event).ToList());
                    await session.SaveChangesAsync();
                });
            }

            if (eventStoreOutcome.Outcome == OutcomeType.Successful)
            {
                aggregate.Version += aggregate.Events.Count();

                var aggregateSnapshot = _context.GetRequiredService<IAggregateSnapshot<TAggregate>>();
                await aggregateSnapshot.SaveAsync(streamName, aggregate, TimeSpan.FromDays(7));
            }
        }

        public Task DeleteAsync<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregateRoot
        {
            throw new NotImplementedException();
        }

        public async Task<TAggregate> GetByIdOrDefaultAsync<TAggregate>()
            where TAggregate : class, IAggregateRoot, new()
        {
            return await GetByIdOrDefaultAsync<TAggregate>(string.Empty, 0);
        }

        public async Task<TAggregate> GetByIdOrDefaultAsync<TAggregate>(string aggregateId)
            where TAggregate : class, IAggregateRoot, new()
        {
            return await GetByIdOrDefaultAsync<TAggregate>(aggregateId, 0);
        }

        private async Task<TAggregate> GetByIdOrDefaultAsync<TAggregate>(string aggregateId, int version)
            where TAggregate : class, IAggregateRoot, new()
        {
            var streamName = $"{typeof(TAggregate).Name}:{aggregateId}";

            if (version < 0)
            {
                throw new InvalidOperationException("Cannot get version <= 0");
            }

            TAggregate aggregate = new TAggregate
            {
                Version = 0
            };

            var aggregateSnapshot = _context.GetRequiredService<IAggregateSnapshot<TAggregate>>();

            var aggregateFromCache = await aggregateSnapshot.LoadAsync(streamName);
            if (aggregateFromCache is not null)
            {
                aggregate = aggregateFromCache;
            }

            long sliceStart = aggregate.Version + 1;

            using var session = _eventStore.LightweightSession();
            var events = await session.Events.FetchStreamAsync(streamName, version: version, fromVersion: sliceStart);

            foreach (var @event in events)
            {
                object eventObject = DeserializeEvent(@event);
                Type applyType = typeof(IApply<>).MakeGenericType(eventObject.GetType());
                var isAssignableFrom = applyType.IsInstanceOfType(aggregate);
                if (isAssignableFrom)
                {
                    ((dynamic)aggregate).Apply((dynamic)eventObject);
                }
                aggregate.Version++;
            }

            aggregate.Id = aggregateId;
            aggregate.Context = _context;
            return aggregate;
        }

        private object DeserializeEvent(IEvent @event)
        {
            if (@event.DotNetTypeName == null)
            {
                throw new NullReferenceException("EventTypeName is null");
            }

            return @event.Data;
        }
    }
}