namespace BricksHoarder.DateTime.Noda
{
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