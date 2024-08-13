using System;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var str = @"
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
    public static bool IsEqualAnyIgnoreCase(this string strA, params string[] candidateStrings)
    {
        if ((strA == null && candidateStrings == null) ||
            (candidateStrings == null)
            )
        {
            return false;
        }
        var comparisonType = StringComparison.OrdinalIgnoreCase;
        return candidateStrings.Any(str => str.Equals(strA, comparisonType));
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
        query.Where(a => a.Name.IsEqualAnyIgnoreCase(""abc""));
    }
}
";

            Console.WriteLine(str.Substring(963, 997 - 963));

            Console.WriteLine("Hello, World!");
        }
    }
}