namespace BricksHoarder.Core.Services
{
    public interface IGuidService
    {
        Guid New { get; }
    }

    public interface IRandomService
    {
        DateTime Between(DateTime start, DateTime end);
    }
}