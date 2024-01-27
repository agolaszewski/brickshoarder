namespace BricksHoarder.DateTime
{
    public static class DateTimeExtensions
    {
        public static Guid ToGuid(this System.DateTime dateTime)
        {
            var bytes = BitConverter.GetBytes(dateTime.Ticks);
            Array.Resize(ref bytes, 16);
            return new Guid(bytes);
        }
    }
}