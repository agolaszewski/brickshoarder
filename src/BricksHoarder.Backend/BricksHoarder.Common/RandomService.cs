using BricksHoarder.Core.Services;

namespace BricksHoarder.Common
{
    public class RandomService : IRandomService
    {
        private readonly Random _rng = new();

        public DateTime Between(DateTime start, DateTime end)
        {
            if (start >= end)
            {
                throw new ArgumentException("start cannot be greater then end");
            }

            var next = _rng.NextInt64(start.Ticks, end.Ticks);
            return new DateTime(next);
        }
    }
}