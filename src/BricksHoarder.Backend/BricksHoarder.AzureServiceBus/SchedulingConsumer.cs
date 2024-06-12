using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Azure.ServiceBus
{
    public class SchedulingConsumer<TCommand, TEvent> (ILogger<SchedulingConsumer<TCommand, TEvent>> logger) : IConsumer<TEvent> where TEvent : class, IEvent, IScheduling<TCommand> where TCommand : class, ICommand
    {
        public async Task Consume(ConsumeContext<TEvent> context)
        {
            try
            {
                logger.LogDebug("Consuming {Message} {CorrelationId}", context.Message.GetType().FullName, context.CorrelationId);
                var schedulingDetails = context.Message.SchedulingDetails();
                await context.ScheduleSend(schedulingDetails.QueueName, schedulingDetails.ScheduleTime, schedulingDetails.Command, Pipe.Execute<SendContext<TCommand>>(x => x.CorrelationId = context.CorrelationId));
            }
            catch
            {
                logger.LogError("Exception In SchedulingConsumer {Message} {CorrelationId} {Content}", context.Message.GetType().FullName, context.CorrelationId, System.Text.Json.JsonSerializer.Serialize(context.Message));
                throw;
            }
            finally
            {
                logger.LogDebug("Consuming {Message} {CorrelationId}", context.Message.GetType().FullName, context.CorrelationId);
            }
        }
    }
}