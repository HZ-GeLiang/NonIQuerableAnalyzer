using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = NonAnalyzer.Test.CSharpCodeFixVerifier<
    NonAnalyzer.EnumEqualsAnalyzer,
    NonAnalyzer.NonAnalyzerCodeFixProvider>;

namespace NonAnalyzer.Test
{
    [TestClass]
    public class EnumObjTest
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

class Program
{
    public enum Status
    {
        Active,
        Inactive,
        Pending
    }

    static void Main()
    {
        Status status = Status.Active;
        //器会提示警告，建议改用 == 而不是 .Equals。
        if (status.Equals(Status.Inactive)) {}
        if (status.Equals(1)){ }
    }
}

");
        }
    }
}