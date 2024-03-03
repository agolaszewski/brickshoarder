using NodaTime;

namespace BricksHoarder.DateTime.Noda
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public System.DateTime UtcNow()
        {
            return SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
        }

        public System.DateTime LocalNow(TimeZoneId timeZoneId)
        {
            var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId.Value);
            return SystemClock.Instance.GetCurrentInstant().InZone(timeZone!).ToDateTimeUnspecified();
        }
    }
}