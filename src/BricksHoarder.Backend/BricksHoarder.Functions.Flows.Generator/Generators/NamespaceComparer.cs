namespace BricksHoarder.Functions.Flows.Generator.Generators
{
    public class NamespaceComparer : IComparer<string>
    {
        public int Compare(string? left, string? right)
        {
            if (left!.StartsWith("BricksHoarder") && !right!.StartsWith("BricksHoarder"))
            {
                return -1;
            }

            if (!left.StartsWith("BricksHoarder") && right!.StartsWith("BricksHoarder"))
            {
                return 1;
            }

            if (!left.StartsWith("BricksHoarder") && !right!.StartsWith("BricksHoarder"))
            {
                return string.CompareOrdinal(left, right);
            }

            return string.CompareOrdinal(left[14..], right![14..]);
        }
    }
}