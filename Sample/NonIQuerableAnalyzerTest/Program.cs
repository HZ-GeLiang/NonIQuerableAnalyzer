using System.Runtime.Intrinsics.X86;

namespace NonIQuerableAnalyzerTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            {
                IQueryable<int> query = null;
                //var queryableResult = query.OrderByExpression(1);  // error
            }

            {
                IQueryable<Stu> query = null;
                //query.Where(a => a.Name.IsEqualAnyIgnoreCase("abc")); //error
            }

            {
                IQueryable<Stu> query = null;
                query.WhereIf("".HasValue(), a => a.Name == "1"); //ok , "".HasValue() 不会报错
            }

            {
                IQueryable<Stu> query = null;
                query.WhereIf("".HasValue(), a => a.Name.IsEqualAnyIgnoreCase("abc")); //error
            }
        }

        class Stu
        {
            public string Name { get; set; }
        }
    }
}