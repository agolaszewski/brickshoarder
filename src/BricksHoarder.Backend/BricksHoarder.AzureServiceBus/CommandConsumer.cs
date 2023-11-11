using BricksHoarder.Core.Aggregates;
using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.AzureServiceBus
{
    public class CommandConsumer<TCommand, TAggregateRoot> : IConsumer<TCommand> where TCommand : class, ICommand where TAggregateRoot : class, IAggregateRoot
    {
        private readonly ICommandHandler<TCommand, TAggregateRoot> _handler;
        private readonly IAggregateStore _aggregateStore;
        private readonly ILogger<CommandConsumer<TCommand, TAggregateRoot>> _logger;
        private readonly IIntegrationEventsQueue _integrationEventsQueue;

        public CommandConsumer(
            ICommandHandler<TCommand, TAggregateRoot> handler,
            IAggregateStore aggregateStore,
            ILogger<CommandConsumer<TCommand, TAggregateRoot>> logger,
            IIntegrationEventsQueue integrationEventsQueue)
        {
            _handler = handler;
            _aggregateStore = aggregateStore;
            _logger = logger;
            _integrationEventsQueue = integrationEventsQueue;
        }

        public async Task Consume(ConsumeContext<TCommand> context)
        {
            try
            {
                _logger.LogDebug($"Consuming {context.Message.GetType().FullName} {context.CorrelationId}");

                TAggregateRoot aggregateRoot = await _handler.HandleAsync(context.Message);
                await _aggregateStore.SaveAsync(aggregateRoot);

                foreach (var @event in aggregateRoot.Events)
                {
                    await context.Publish(@event.Event, @event.Event.GetType(), x => x.CorrelationId = context.CorrelationId);
                }

                foreach (var @event in _integrationEventsQueue.Events)
                {
                    await context.Publish(@event, @event.GetType(), x => x.CorrelationId = context.CorrelationId);
                }

                await context.Publish(new CommandConsumed<TCommand>(context.Message, typeof(TCommand).FullName!), x => x.CorrelationId = context.CorrelationId);
            }
            catch (Exception e)
            {
                int x = 1;
            }
            finally
            {
                _logger.LogDebug($"Consumed {context.Message.GetType().FullName} {context.CorrelationId!}");
            }
        }
    }

    //public class CommandConsumer<TCommand> : IConsumer<TCommand> where TCommand : class, ICommand
    //{
    //    //private readonly IExceptionHandler _exceptionHandler;
    //    private readonly ICommandHandler<TCommand> _handler;

    //    //private readonly IEventFactory _eventFactory;
    //    private readonly IAggregateStore _aggregateStore;

    //    private readonly ILogger<CommandConsumer<TCommand>> _logger;
    //    private readonly IIntegrationEventsQueue _integrationEventsQueue;
    //    private readonly ISendEndpointProvider _sendEndpointProvider;

    //    public CommandConsumer(
    //        ICommandHandler<TCommand> handler,
    //        //IEventFactory eventFactory,
    //        //IExceptionHandler exceptionHandler,
    //        IAggregateStore aggregateStore,
    //        IIntegrationEventsQueue integrationEventsQueue,
    //        ISendEndpointProvider sendEndpointProvider,
    //        ILogger<CommandConsumer<TCommand>> logger)
    //    {
    //        _handler = handler;
    //        //_eventFactory = eventFactory;
    //        //_exceptionHandler = exceptionHandler;
    //        _aggregateStore = aggregateStore;
    //        _logger = logger;
    //        _integrationEventsQueue = integrationEventsQueue;
    //        _sendEndpointProvider = sendEndpointProvider;
    //    }

    //    public async Task Consume(ConsumeContext<TCommand> context)
    //    {
    //        try
    //        {
    //            _logger.LogDebug($"Consuming {context.Message.GetType().FullName} {context.CorrelationId}");

    //            IAggregateRoot aggregateRoot = await _handler.ExecuteAsync(context.Message);
    //            //_eventFactory.Make(aggregateRoot.Events);

    //            await aggregateRoot.CommitAsync(_aggregateStore);

    //            //ISendEndpoint endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:events"));

    //            //foreach (var composite in aggregateRoot.Events)
    //            //{
    //            //    await endpoint.Send(composite.Event, composite.Event.GetType());
    //            //}

    //            //foreach (var @event in _integrationEventsQueue.Events)
    //            //{
    //            //    await endpoint.Send(@event, @event.GetType());
    //            //}

    //            //await context.RespondAsync(new Response(aggregateRoot.Id));
    //        }
    //        catch (AppValidationException ex)
    //        {
    //            await context.Publish(new ValidationExceptionRaised(ex, context.CorrelationId.Value));
    //            await context.RespondAsync(new BadRequestResponse(ex));
    //        }
    //        catch (Exception ex)
    //        {
    //            await context.Publish(new UnhandledExceptionOccured() { CorrelationId = context.CorrelationId.Value, CommandFullName = context.Message.GetType().FullName!, Message = ex.Message });
    //            //var exceptionId = _exceptionHandler.Handle(ex, context.Message);
    //            //await context.RespondAsync(message: new ErrorResponse(exceptionId));
    //        }
    //        finally
    //        {
    //            _logger.LogDebug($"Consumed {context.Message.GetType().FullName} {context.CorrelationId}");
    //        }
    //    }
    //}
}