using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = NonIQuerableAnalyzer.Test.CSharpCodeFixVerifier<
    NonIQuerableAnalyzer.NonIQuerable_Analyzer,
    NonIQuerableAnalyzer.NonIQuerableAnalyzerCodeFixProvider>;

namespace NonIQuerableAnalyzer.Test
{
    [TestClass]
    public class WhereIfTest
    {
        [TestMethod]
        public async Task Test_OnClass()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class NonIQueryableAttribute : Attribute { }

[NonIQueryableAttribute]
public static partial class StringExtensions
{
    public static bool HasValue(this string value)
    {
        return !string.IsNullOrEmpty(value);
    }
}

class Program
{
    class Stu
    {
        public string Name { get; set; }
    }
    static void Main()
    {
        IQueryable<Stu> query = null;
        query.WhereIf("""".HasValue(), a => a.Name == ""1"");
    }
}
public static class IQueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }
}
");
        }
    }
}