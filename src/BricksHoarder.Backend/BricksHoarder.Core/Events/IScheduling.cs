using BricksHoarder.Core.Commands;

namespace BricksHoarder.Core.Events
{
    public interface IScheduling<TCommand> where TCommand : ICommand
    {
        SchedulingDetails<TCommand> SchedulingDetails();
    }
}