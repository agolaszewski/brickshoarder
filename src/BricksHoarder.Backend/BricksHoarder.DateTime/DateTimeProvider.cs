using NodaTime;

namespace BricksHoarder.DateTime
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public System.DateTime UtcNow()
        {
            return SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
        }
    }
}