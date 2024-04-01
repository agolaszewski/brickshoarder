using BricksHoarder.Core.Commands;

namespace BricksHoarder.Core.Events
{
    public interface IEvent
    {
    }

    public interface ISchedulingEvent : IEvent
    {
        public SchedulingDetails<TCommand> Scheduling<TCommand>() where TCommand : ICommand;
    }

    public record SchedulingDetails<TCommand> where TCommand : ICommand
    {
        public SchedulingDetails(TCommand command, Uri queueName, DateTime scheduleTime)
        {
            ScheduleTime = scheduleTime;
            QueueName = queueName;
            Command = command;
        }

        public TCommand Command { get; }

        public Uri QueueName { get; }

        public DateTime ScheduleTime { get; }
    }
}