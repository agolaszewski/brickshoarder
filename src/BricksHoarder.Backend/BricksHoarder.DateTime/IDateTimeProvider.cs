namespace BricksHoarder.DateTime.Noda
{
    public interface IDateTimeProvider
    {
        System.DateTime UtcNow();

        System.DateTime LocalNow(TimeZoneId timeZoneId);
    }
}