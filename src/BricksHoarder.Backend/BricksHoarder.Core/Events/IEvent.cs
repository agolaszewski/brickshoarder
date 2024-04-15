using BricksHoarder.Core.Commands;

namespace BricksHoarder.Core.Events
{
    public interface IEvent
    {
    }

    public interface IScheduling<TCommand> where TCommand : ICommand
    {
        SchedulingDetails<TCommand> SchedulingDetails();
    }

    public record SchedulingDetails<TCommand>(string Id, TCommand Command, Uri QueueName, DateTime ScheduleTime) where TCommand : ICommand;
}