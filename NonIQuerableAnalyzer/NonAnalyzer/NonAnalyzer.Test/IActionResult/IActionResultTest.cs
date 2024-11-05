using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = NonAnalyzer.Test.CSharpCodeFixVerifier<
    NonAnalyzer.IActionResultNullReturnAnalyzer,
    NonAnalyzer.NonAnalyzerCodeFixProvider>; // 确保此处是你的分析器和代码修复程序

namespace NonAnalyzer.Test
{
    [TestClass]
    public class IActionResultTest
    {
        //public async Task<IActionResult> GetFile()
        //{
        //    await Task.Delay(1);
        //    return null; // 这里将触发分析器的警告
        //}

        [TestMethod]
        public async Task Test_IActionResult()
        {
            var testCode = @"
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
    }

    public async Task<IActionResult> GetFile()
    {
        await Task.Delay(1);
        return null; // 这里将触发分析器的警告
    }
}";

            //await VerifyCS.VerifyAnalyzerAsync(@"");

            var test = new VerifyCS.Test
            {
                TestCode = testCode,
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp31, // 使用 .NET Core 3.1 参考程序集
            };

            // 添加 `Microsoft.AspNetCore.Mvc` 依赖程序集
            test.TestState.AdditionalReferences.Add(typeof(Microsoft.AspNetCore.Mvc.IActionResult).Assembly);

            await test.RunAsync();
        }
    }
}