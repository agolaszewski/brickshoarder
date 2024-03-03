namespace BricksHoarder.DateTime.Noda
{
    public interface IDateTimeProvider
    {
        System.DateTime UtcNow();

        System.DateTime LocalNow(TimeZoneId timeZoneId);
    }

    public class TimeZoneId
    {
        public string Value { get; }

        private TimeZoneId(string value)
        {
            Value = value;
        }

        public static TimeZoneId Poland => new TimeZoneId("Europe/Warsaw");
    }
}