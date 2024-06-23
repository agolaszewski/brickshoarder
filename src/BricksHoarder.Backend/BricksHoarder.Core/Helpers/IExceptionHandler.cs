namespace BricksHoarder.Core.Helpers
{
    public interface IExceptionHandler
    {
        Guid Handle<T>(Exception exception, T body);
    }
}