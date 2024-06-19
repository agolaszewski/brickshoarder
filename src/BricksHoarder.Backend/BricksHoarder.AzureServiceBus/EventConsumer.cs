using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using BricksHoarder.Core.Helpers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Azure.ServiceBus;

public abstract class EventConsumer<TEvent>(ILogger<EventConsumer<TEvent>> logger) : IConsumer<TEvent> where TEvent : class, IEvent
{
    public abstract Task<IReadOnlyList<ICommand>> HandleAsync(TEvent @event);

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        try
        {
            logger.LogDebug($"Consuming {context.Message.GetType().FullName} {context.CorrelationId}");
            IReadOnlyList<ICommand> commands = await HandleAsync(context.Message);

            foreach (var command in commands)
            {
                await context.Send(PathHelper.QueuePathUri(command), command, x => x.CorrelationId = context.CorrelationId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception In CommandConsumer {Message} {CorrelationId} {Content}", context.Message.GetType().FullName, context.CorrelationId, System.Text.Json.JsonSerializer.Serialize(context.Message));
            throw;
        }
        finally
        {
            logger.LogDebug($"Consumed {context.Message.GetType().FullName} {context.CorrelationId}");
        }
    }
}