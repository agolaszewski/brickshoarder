using BricksHoarder.Core.Services;

namespace BricksHoarder.Common
{
    public class GuidService : IGuidService
    {
        public Guid New => Guid.NewGuid();
    }

    public class RandomService : IRandomService
    {
        private readonly Random _rng;

        public RandomService()
        {
            _rng = new Random();
        }

        public DateTime Between(DateTime start, DateTime end)
        {
            if (end >= start)
            {
                throw new ArgumentException("end cannot be greater then start");
            }

            var next = _rng.NextInt64(start.Ticks, end.Ticks);
            return new DateTime(next);
        }
    }
}