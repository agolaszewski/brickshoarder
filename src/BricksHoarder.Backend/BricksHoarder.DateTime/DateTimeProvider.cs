using NodaTime;

namespace BricksHoarder.DateTime.Noda
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public System.DateTime UtcNow()
        {
            return SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
        }
    }
}