namespace BricksHoarder.Core.Commands
{
    public interface ICommand
    {
        Guid CorrelationId { get; init; }
    }
}
