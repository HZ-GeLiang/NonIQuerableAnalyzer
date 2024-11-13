using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
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

        private VerifyCS.Test GetTest(string testCode)
        {
            var test = new VerifyCS.Test
            {
                TestCode = testCode,
                ReferenceAssemblies = ReferenceAssemblies.NetCore.NetCoreApp31, // 使用 .NET Core 3.1 参考程序集
            };

            // 添加 `Microsoft.AspNetCore.Mvc` 依赖程序集
            test.TestState.AdditionalReferences.Add(typeof(Microsoft.AspNetCore.Mvc.IActionResult).Assembly);

            //当前行的程序集应该和上面是一样的, 实测下来: 如果没有不写这行, type.TypeKind 为 TypeKind.Error.
            test.TestState.AdditionalReferences.Add(typeof(Microsoft.AspNetCore.Mvc.FileStreamResult).Assembly);

            return test;
        }

        [TestMethod]
        public async Task Test1_Task_IActionResult()
        {
            var testCode = @"
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

class Program
{
    public async Task<IActionResult> GetFile1()
    {
        await Task.Delay(1);
        return null; // 这里将触发分析器的警告
    }
}";
            var test = GetTest(testCode);
            await test.RunAsync();
        }

        [TestMethod]
        public async Task Test2_IActionResult()
        {
            var testCode = @"
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

class Program
{
    public IActionResult GetFile2()
    {
        return null; // 这里将触发分析器的警告
    }
}";

            var test = GetTest(testCode);
            await test.RunAsync();
        }

        [TestMethod]
        public async Task Test3_Task_FileStreamResult()
        {
            //需要确保当前程序集中有 FileStreamResult
            var testCode = @"
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

class Program
{
    public async Task<FileStreamResult> GetFile3()
    {
        await Task.Delay(1);
        return null; // 这里将触发分析器的警告
    }
}";

            var test = GetTest(testCode);
            await test.RunAsync();
        }

        [TestMethod]
        public async Task Test4_FileStreamResult()
        {
            var testCode = @"
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

class Program
{
    public FileStreamResult GetFile4()
    {
        return null; // 这里将触发分析器的警告
    }
}";

            var test = GetTest(testCode);
            await test.RunAsync();
        }

        [TestMethod]
        public async Task Test5_FileStreamResult_return_default()
        {
            var testCode = @"
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

class Program
{
    public FileStreamResult GetFile5()
    {
        return default; // 这里将触发分析器的警告
    }
}";

            var test = GetTest(testCode);
            await test.RunAsync();
        }
    }
}