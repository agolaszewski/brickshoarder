using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Core.Exceptions;
using BricksHoarder.Core.Specification;
using BricksHoarder.Events;
using EventStore.Client;
using FluentValidation.Results;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Polly;
using StackExchange.Redis;
using JsonSerializer = System.Text.Json.JsonSerializer;
using StreamPosition = EventStore.Client.StreamPosition;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace RealWorld.EventStore
{
    public class EventStoreAggregateStore : IAggregateStore
    {
        private readonly IDatabase _cache;

        private readonly IServiceProvider _context;

        private readonly EventStoreClient _eventStoreClient;

        private readonly IPublishEndpoint _publishEndpoint;

        public EventStoreAggregateStore(
            EventStoreClient eventStoreClient,
            IDatabase cache,
            IServiceProvider context,
            IPublishEndpoint publishEndpoint)
        {
            _eventStoreClient = eventStoreClient;
            _cache = cache;
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid aggregateId) where TAggregate : class, IAggregateRoot, new()
        {
            var aggregate = await GetByIdOrDefaultAsync<TAggregate>(aggregateId.ToString(), int.MaxValue);
            if (aggregate is null)
            {
                throw new AppValidationException(aggregateId, $"{typeof(TAggregate).Name}Id", $"Aggregate with id : {aggregateId} not found");
            }

            return aggregate;
        }

        public async Task<TAggregate> GetByIdAsync<TAggregate, TCommand>(Guid aggregateId, TCommand command) where TAggregate : class, IAggregateRoot, new() where TCommand : ICommand
        {
            var aggregate = await GetByIdOrDefaultAsync<TAggregate>(aggregateId.ToString(), int.MaxValue);
            if (aggregate is null)
            {
                throw new AppValidationException(aggregateId, $"{typeof(TAggregate).Name}Id", $"Aggregate with id : {aggregateId} not found");
            }

            var specificationFor = _context.GetService<ISpecificationForCommand<TAggregate, TCommand>>();

            if (specificationFor is not null)
            {
                var validator = specificationFor.Apply(command);
                ValidationResult result = await validator.ValidateAsync(aggregate);
                if (!result.IsValid)
                {
                    throw new AppValidationException(aggregateId, result.Errors);
                }
            }

            return aggregate;
        }

        public TAggregate GetNew<TAggregate>() where TAggregate : class, IAggregateRoot, new()
        {
            var model = new TAggregate { Context = _context, Version = -1 };
            return model;
        }

        public async Task SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregateRoot
        {
            string streamName = $"{aggregate.GetType().Name}:{aggregate.Id}";
            IEnumerable<EventComposite> newEvents = aggregate.Events;
            List<EventData> eventsToSave = newEvents.Select(ToEventData).ToList();

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

            var result = await Policies.EventStoreRetryPolicy.ExecuteAndCaptureAsync(() => _eventStoreClient.AppendToStreamAsync(streamName, StreamState.Any, eventsToSave));
            if (result.Outcome == OutcomeType.Successful)
            {
                aggregate.Version += aggregate.Events.Count() - 1;
                await Policies.RedisFallbackPolicy.ExecuteAsync(() => _cache.StringSetAsync(streamName, JsonSerializer.Serialize(aggregate), TimeSpan.FromDays(1)));
                return;
            }

            if (aggregateMap != null)
            {
                var publishResult = await Policies.PublishRetryPolicy.ExecuteAndCaptureAsync(() => _publishEndpoint.Publish(new AggregateInOutOfSyncState()
                {
                    AggregateId = aggregate.Id,
                    Type = aggregate.GetType().FullName,
                    Version = aggregate.Version
                }));

                if (publishResult.Outcome == OutcomeType.Failure)
                {
                    throw new ApplicationException($"Cannot sync aggregate {aggregate.Id} of type {aggregate.GetType().FullName}", result.FinalException);
                }

                throw result.FinalException;
            }
        }

        public async Task DeleteAsync<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregateRoot
        {
            string streamName = $"{aggregate.GetType().Name}:{aggregate.Id}";
            await Policies.RedisFallbackPolicy.ExecuteAsync(() => _cache.KeyDeleteAsync(streamName));

            var aggregateMap = _context.GetService<IAggregateMap<TAggregate>>();
            if (aggregateMap != null)
            {
                await Policies.SqRetryPolicy.ExecuteAsync(() => aggregateMap.DeleteAsync(aggregate));
            }

            var result = await Policies.EventStoreRetryPolicy.ExecuteAndCaptureAsync(() => _eventStoreClient.TombstoneAsync(streamName, StreamRevision.FromInt64(aggregate.Version)));
            if (result.Outcome == OutcomeType.Successful)
            {
                await Policies.RedisFallbackPolicy.ExecuteAsync(() => _cache.KeyDeleteAsync(streamName));
                return;
            }

            if (aggregateMap != null)
            {
                var publishResult = await Policies.PublishRetryPolicy.ExecuteAndCaptureAsync(() => _publishEndpoint.Publish(new AggregateInOutOfSyncState()
                {
                    AggregateId = aggregate.Id,
                    Type = aggregate.GetType().FullName,
                    Version = aggregate.Version
                }));

                if (publishResult.Outcome == OutcomeType.Failure)
                {
                    throw new AggregateInOutOfSyncException(aggregate, result.FinalException);
                }

                throw result.FinalException;
            }
        }

        private object DeserializeEvent(ReadOnlyMemory<byte> metadata, ReadOnlyMemory<byte> data)
        {
            JToken eventType = JObject.Parse(Encoding.UTF8.GetString(metadata.Span)).Property("EventType")?.Value;
            if (eventType == null)
            {
                throw new NullReferenceException("EventType is null");
            }

            string json = Encoding.UTF8.GetString(data.Span);
            return JsonSerializer.Deserialize(json, Type.GetType((string)eventType));
        }

        public Task<TAggregate> GetByIdOrDefaultAsync<TAggregate>(Guid aggregateId)
            where TAggregate : class, IAggregateRoot, new()
        {
            return GetByIdOrDefaultAsync<TAggregate>(aggregateId.ToString(), int.MaxValue);
        }

        private async Task<TAggregate> GetByIdOrDefaultAsync<TAggregate>(string aggregateId, int version)
            where TAggregate : class, IAggregateRoot, new()
        {
            if (version <= 0)
            {
                throw new InvalidOperationException("Cannot get version <= 0");
            }

            var streamName = $"{typeof(TAggregate).Name}:{aggregateId}";
            TAggregate aggregate = new TAggregate
            {
                Version = -1
            };

            RedisValue value = await Policies.RedisValueFallbackPolicy.ExecuteAsync(() => _cache.StringGetAsync(streamName));
            if (value.HasValue)
            {
                aggregate = JsonSerializer.Deserialize<TAggregate>(value);
            }

            long sliceStart = aggregate.Version + 1;
            EventStoreClient.ReadStreamResult stream;

            try
            {
                stream = _eventStoreClient.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.FromInt64(sliceStart));
                if (await stream.ReadState == ReadState.StreamNotFound)
                {
                    return null;
                }
            }
            catch (StreamDeletedException)
            {
                return null;
            }

            await foreach (var @event in stream)
            {
                object eventObject = DeserializeEvent(@event.Event.Metadata, @event.Event.Data);
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

        private EventData ToEventData(EventComposite @event)
        {
            var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event.Event, @event.Event.GetType()));
            var metadata = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event.Metadata));

            return new EventData(@event.Metadata.EventId, @event.Metadata.EventType, data, metadata);
        }
    }
}
