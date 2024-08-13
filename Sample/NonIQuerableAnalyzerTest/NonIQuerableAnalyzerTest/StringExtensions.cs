namespace NonIQuerableAnalyzerTest
{
    [NonIQueryable]
    public static partial class StringExtensions
    {
        public static bool IsEqualAnyIgnoreCase(this string strA, params string[] candidateStrings)
        {
            if ((strA == null && candidateStrings == null) || (candidateStrings == null))
            {
                return false;
            }
            var comparisonType = StringComparison.OrdinalIgnoreCase;
            return candidateStrings.Any(str => str.Equals(strA, comparisonType));
        }

        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }
    }
}