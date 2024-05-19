using BricksHoarder.Core.Commands;

namespace BricksHoarder.Core.Events
{
    public record SchedulingDetails<TCommand>(string Id, TCommand Command, Uri QueueName, DateTime ScheduleTime) where TCommand : ICommand;
}