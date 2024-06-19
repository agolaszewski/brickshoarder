using BricksHoarder.Core.Commands;

namespace BricksHoarder.Core.Helpers
{
    public static class PathHelper
    {
        public static Uri QueuePathUri<TCommand>() where TCommand : ICommand
        {
            return new Uri($"queue:{typeof(TCommand).Name}");
        }

        public static Uri QueuePathUri(ICommand command)
        {
            return new Uri($"queue:{command.GetType().Name}");
        }
    }
}