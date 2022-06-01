using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoMapper;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Core.Exceptions;
using BricksHoarder.Core.Specification;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;


namespace RealWorld.Common.DDD.Aggregates
{
    public abstract class AggregateRoot<TAggregate> : IAggregateRoot
        where TAggregate : AggregateRoot<TAggregate>
    {
        private readonly List<EventComposite> _events = new List<EventComposite>();

        [JsonIgnore]
        public IServiceProvider Context { get; set; }

        [JsonIgnore]
        public IEnumerable<EventComposite> Events => _events;

        public long Version { get; set; }

        public Guid Id { get; set; }

        public bool ToDelete { get; set; }

        public bool IsDeleted { get; set; }

        protected void Validate<TCommand>(TCommand command) where TCommand : ICommand
        {
            var specification = Context.GetService<ISpecificationForCommand<TAggregate, TCommand>>();
            if (specification is null)
            {
                throw new NotImplementedException($"{command.GetType().FullName} doesn't have validator implemented");
            }

            IValidator<TAggregate> validator = specification.Apply(command);
            ValidationResult result = validator.Validate(this as TAggregate);
            if (!result.IsValid)
            {
                throw new AppValidationException(Id, result.Errors);
            }
        }

        protected void AddEvent<TCommand, TEvent>(TCommand command) where TCommand : ICommand where TEvent : IEvent
        {
            var mapper = Context.GetService<IMapper>();
            var @event = mapper.Map<TEvent>(command);
            AddEvent(@event);
        }

        protected void AddEvent<TEvent>(TEvent @event) where TEvent : IEvent
        {
            (this as IApply<TEvent>)?.Apply(@event);

            var specificationFor = Context.GetService<ISpecificationForEvent<TAggregate, TEvent>>();

            if (specificationFor is not null)
            {
                IValidator<TAggregate> validator = specificationFor.Apply(@event);
                ValidationResult result = validator.Validate(this as TAggregate);
                if (!result.IsValid)
                {
                    throw new AppValidationException(Id, result.Errors);
                }
            }

            _events.Add(new EventComposite(@event));
        }

        public virtual async Task CommitAsync(IAggregateStore aggregateStore)
        {
            if (ToDelete)
            {
                await aggregateStore.DeleteAsync(this as TAggregate);
                return;
            }
            await aggregateStore.SaveAsync(this as TAggregate);
        }

        public virtual void Delete()
        {
            ToDelete = true;
        }
    }
}
