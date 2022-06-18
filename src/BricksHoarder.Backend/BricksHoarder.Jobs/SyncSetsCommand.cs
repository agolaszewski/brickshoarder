using BricksHoarder.Core.Commands;

namespace BricksHoarder.Jobs
{
    public record SyncSetsCommand : ICommand
    {
        public int PageNumber { get; set; }

        public Guid CorrelationId { get; set; }
    }
}