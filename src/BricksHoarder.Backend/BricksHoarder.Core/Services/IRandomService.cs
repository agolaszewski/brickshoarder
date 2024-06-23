namespace BricksHoarder.Core.Services
{
    public interface IRandomService
    {
        DateTime Between(DateTime start, DateTime end);
    }
}