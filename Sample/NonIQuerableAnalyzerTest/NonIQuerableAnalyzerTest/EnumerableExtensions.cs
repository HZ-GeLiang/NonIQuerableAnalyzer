namespace NonIQuerableAnalyzerTest
{
    [NonIQueryableAttribute]
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> OrderByExpression<TSource>(this IEnumerable<TSource> source, int a)
        {
            return source;
        }
    }
}