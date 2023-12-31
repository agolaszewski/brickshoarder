using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.AzureCloud.ServiceBus;

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