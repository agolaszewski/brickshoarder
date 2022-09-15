using BricksHoarder.Common.CQRS.Responses;
using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Core.Exceptions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Response = BricksHoarder.Common.CQRS.Responses.Response;

namespace BricksHoarder.RabbitMq
{
    public class CommandConsumer<TCommand> : IConsumer<TCommand> where TCommand : class, ICommand
    {
        //private readonly IExceptionHandler _exceptionHandler;
        private readonly ICommandHandler<TCommand> _handler;

        //private readonly IEventFactory _eventFactory;
        private readonly IAggregateStore _aggregateStore;

        private readonly ILogger<CommandConsumer<TCommand>> _logger;

        public CommandConsumer(
            ICommandHandler<TCommand> handler,
            //IEventFactory eventFactory,
            //IExceptionHandler exceptionHandler,
            IAggregateStore aggregateStore,
            ILogger<CommandConsumer<TCommand>> logger)
        {
            _handler = handler;
            //_eventFactory = eventFactory;
            //_exceptionHandler = exceptionHandler;
            _aggregateStore = aggregateStore;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TCommand> context)
        {
            try
            {
                _logger.LogDebug($"Consuming {context.Message.GetType().FullName} {context.CorrelationId}");

                IAggregateRoot aggregateRoot = await _handler.ExecuteAsync(context.Message);
                //_eventFactory.Make(aggregateRoot.Events);

                await aggregateRoot.CommitAsync(_aggregateStore);

                foreach (var composite in aggregateRoot.Events)
                {
                    await context.Publish(composite.Event, composite.Event.GetType());
                }

                await context.RespondAsync(new Response(aggregateRoot.Id));
            }
            catch (AppValidationException ex)
            {
                await context.Publish(new ValidationExceptionRaised(ex, context.CorrelationId.Value));
                await context.RespondAsync(new BadRequestResponse(ex));
            }
            catch (Exception ex)
            {
                await context.Publish(new UnhandledExceptionOccured() { CorrelationId = context.CorrelationId.Value, CommandFullName = context.Message.GetType().FullName!, Message = ex.Message });
                //var exceptionId = _exceptionHandler.Handle(ex, context.Message);
                //await context.RespondAsync(message: new ErrorResponse(exceptionId));
            }
            finally
            {
                _logger.LogDebug($"Consumed {context.Message.GetType().FullName} {context.CorrelationId}");
            }
        }
    }
}