using BricksHoarder.Core.Commands;
using BricksHoarder.Core.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BricksHoarder.Azure.ServiceBus
{
    public class SchedulingConsumer<TCommand, TEvent> : IConsumer<TEvent> where TEvent : class, IEvent, IScheduling<TCommand> where TCommand : class, ICommand
    {
        private readonly ILogger<SchedulingConsumer<TCommand, TEvent>> _logger;

        public SchedulingConsumer(
            ILogger<SchedulingConsumer<TCommand, TEvent>> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TEvent> context)
        {
            try
            {
                _logger.LogDebug($"Consuming {context.Message.GetType().FullName} {context.CorrelationId}");
                var schedulingDetails = context.Message.SchedulingDetails();
                await context.ScheduleSend(schedulingDetails.QueueName, schedulingDetails.ScheduleTime, schedulingDetails.Command, Pipe.Execute<SendContext<TCommand>>(x => x.CorrelationId = context.CorrelationId));
            }
            finally
            {
                _logger.LogDebug($"Consumed {context.Message.GetType().FullName} {context.CorrelationId}");
            }
        }
    }
}