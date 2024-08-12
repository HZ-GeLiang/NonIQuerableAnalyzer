using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = NonIQuerableAnalyzer.Test.CSharpCodeFixVerifier<
    NonIQuerableAnalyzer.NonIQuerable_Analyzer,
    NonIQuerableAnalyzer.NonIQuerableAnalyzerCodeFixProvider>;

namespace NonIQuerableAnalyzer.Test
{
    [TestClass]
    public class IEnumerableUnitTest
    {
        [TestMethod]
        public async Task Test_OnMethod()
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

public static class EnumerableExtensions
{
    [NonIQueryableAttribute]
    public static IEnumerable<TSource> OrderByExpression<TSource>(this IEnumerable<TSource> source, int a)
    {
        return source;
    }
}

class Program
{
    static void Main()
    {
        IQueryable<int> query = null;
        var queryableResult = query.OrderByExpression(1);
    }
}
");
        }

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
public static class EnumerableExtensions
{
    public static IEnumerable<TSource> OrderByExpression<TSource>(this IEnumerable<TSource> source, int a)
    {
        return source;
    }
}

class Program
{
    static void Main()
    {
        IQueryable<int> query = null;
        var queryableResult = query.OrderByExpression(1);
    }
}
");
        }
    }
}