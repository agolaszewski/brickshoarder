namespace BricksHoarder.Core.Helpers
{
    public class ComparerService
    {
        public bool HasChanged { get; private set; } = false;

        public ComparerService Compare<T>(T? a, T? b)
        {
            if (HasChanged)
            {
                return this;
            }

            if (a == null || b == null)
            {
                return this;
            }

            if (a.Equals(b))
            {
                return this;
            }

            HasChanged = true;
            return this;
        }

        public ComparerService CompareStruct<T>(T? a, T? b) where T : struct
        {
            if (HasChanged)
            {
                return this;
            }

            if (a.Equals(b))
            {
                return this;
            }

            HasChanged = true;
            return this;
        }
    }
}