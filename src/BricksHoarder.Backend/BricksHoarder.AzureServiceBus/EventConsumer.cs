using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Azure.ServiceBus;

public class EventConsumer<TEvent> : IConsumer<TEvent> where TEvent : class, IEvent
{
    private readonly IEventHandler<TEvent> _handler;
    private readonly ILogger<EventConsumer<TEvent>> _logger;

    public EventConsumer(
        IEventHandler<TEvent> handler,
        ILogger<EventConsumer<TEvent>> logger)
    {
        _logger = logger;
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        try
        {
            _logger.LogDebug($"Consuming {context.Message.GetType().FullName} {context.CorrelationId}");
            IReadOnlyList<ICommand> commands = await _handler.HandleAsync(context.Message);

            foreach (var command in commands)
            {
                await context.Send(new Uri("queue:commands"), command, x => x.CorrelationId = context.CorrelationId);
            }
        }
        finally
        {
            _logger.LogDebug($"Consumed {context.Message.GetType().FullName} {context.CorrelationId}");
        }
    }
}

public class SchedulingConsumer<TCommand, TEvent> : IConsumer<TEvent> where TEvent : class, ISchedulingEvent where TCommand : class, ICommand
{
    private readonly IEventHandler<TEvent> _handler;
    private readonly ILogger<SchedulingConsumer<TCommand,TEvent>> _logger;

    public SchedulingConsumer(
        IEventHandler<TEvent> handler,
        ILogger<SchedulingConsumer<TCommand, TEvent>> logger)
    {
        _logger = logger;
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        try
        {
            _logger.LogDebug($"Consuming {context.Message.GetType().FullName} {context.CorrelationId}");
            var schedulingDetails = context.Message.Scheduling<TCommand>();
            await context.ScheduleSend(schedulingDetails.QueueName, schedulingDetails.ScheduleTime, schedulingDetails.Command);
        }
        finally
        {
            _logger.LogDebug($"Consumed {context.Message.GetType().FullName} {context.CorrelationId}");
        }
    }
}