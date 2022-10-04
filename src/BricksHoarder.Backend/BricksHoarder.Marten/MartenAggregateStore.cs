﻿using BricksHoarder.Core.Aggregates;
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
            var aggregateMap = _context.GetService<IAggregateMap<TAggregate>>();

            if (aggregateMap != null)
            {
                if (aggregate.Version == -1)
                {
                    await Policies.SqRetryPolicy.ExecuteAsync(() => aggregateMap.CreateAsync(aggregate));
                }
                else if (aggregate.IsDeleted)
                {
                    await Policies.SqRetryPolicy.ExecuteAsync(() => aggregateMap.DeleteAsync(aggregate));
                }
                else
                {
                    await Policies.SqRetryPolicy.ExecuteAsync(() => aggregateMap.UpdateAsync(aggregate));
                }
            }

            if (!aggregate.Events.Any())
            {
                return;
            }

            var eventStoreOutcome = await Policies.EventStoreRetryPolicy.ExecuteAndCaptureAsync(async () =>
            {
                await using var session = _eventStore.OpenSession();
                session.Events.Append(streamName, aggregate.Version + aggregate.Events.Count(), aggregate.Events.Select(a => a.Event).ToList());
                await session.SaveChangesAsync();
            });

            if (eventStoreOutcome.Outcome == OutcomeType.Successful)
            {
                aggregate.Version += aggregate.Events.Count() + 1;

                var aggregateSnapshot = _context.GetRequiredService<IAggragateSnapshot<TAggregate>>();
                await aggregateSnapshot.SaveAsync(streamName, aggregate, TimeSpan.FromHours(1));
                return;
            }

            if (aggregateMap != null)
            {
                var publishResult = await Policies.PublishRetryPolicy.ExecuteAndCaptureAsync(() => _publishEndpoint.Publish(new AggregateInOutOfSyncState()
                {
                    AggregateId = aggregate.Id,
                    Type = aggregate.GetType().FullName!,
                    Version = aggregate.Version
                }));

                if (publishResult.Outcome == OutcomeType.Failure)
                {
                    throw new ApplicationException($"Cannot sync aggregate {aggregate.Id} of type {aggregate.GetType().FullName}", eventStoreOutcome.FinalException);
                }

                throw eventStoreOutcome.FinalException;
            }
        }

        public async Task DeleteAsync<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregateRoot
        {
            throw new NotImplementedException();
        }

        public async Task<TAggregate> GetByIdOrDefaultAsync<TAggregate>()
            where TAggregate : class, IAggregateRoot, new()
        {
            var streamName = typeof(TAggregate).Name + ":";
            return await GetByIdOrDefaultAsync<TAggregate>(streamName, 0);
        }

        public async Task<TAggregate> GetByIdOrDefaultAsync<TAggregate>(string aggregateId)
            where TAggregate : class, IAggregateRoot, new()
        {
            var streamName = $"{typeof(TAggregate).Name}:{aggregateId}";
            return await GetByIdOrDefaultAsync<TAggregate>(streamName, 0);
        }

        private async Task<TAggregate> GetByIdOrDefaultAsync<TAggregate>(string streamName, int version)
            where TAggregate : class, IAggregateRoot, new()
        {
            if (version < 0)
            {
                throw new InvalidOperationException("Cannot get version <= 0");
            }

            TAggregate aggregate = new TAggregate
            {
                Version = 0
            };

            var aggregateSnapshot = _context.GetRequiredService<IAggragateSnapshot<TAggregate>>();

            var aggregateFromCache = await aggregateSnapshot.LoadAsync(streamName);
            if (aggregateFromCache is not null)
            {
                aggregate = aggregateFromCache;
            }

            long sliceStart = aggregate.Version + 1;

            using var session = _eventStore.OpenSession();
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