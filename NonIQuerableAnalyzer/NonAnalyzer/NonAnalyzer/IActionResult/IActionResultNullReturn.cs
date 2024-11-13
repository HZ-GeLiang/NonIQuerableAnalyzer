using System;
using System.Collections.Generic;
using System.Text;

namespace NonAnalyzer
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;
    using System.Linq;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IActionResultNullReturnAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NonIActionResult"; //IActionResultNullReturn
        private static readonly LocalizableString Title = "Avoid returning null from IActionResult methods";
        private static readonly LocalizableString MessageFormat = "Method returning IActionResult should not return null";
        private static readonly LocalizableString Description = "Returning null from a method with IActionResult return type can cause runtime exceptions. Consider returning an appropriate IActionResult, like NotFound or BadRequest.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error, // 设置为 Error 级别
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeReturnStatements, SyntaxKind.ReturnStatement);
        }

        private static void AnalyzeReturnStatements(SyntaxNodeAnalysisContext context)
        {
            var returnStatement = (ReturnStatementSyntax)context.Node;
            var methodDeclaration = returnStatement.FirstAncestorOrSelf<MethodDeclarationSyntax>();

            if (methodDeclaration == null)
                return;

            var returnType = context.SemanticModel.GetTypeInfo(methodDeclaration.ReturnType).ConvertedType;

            if (returnType == null)
                return;

            //var taskType = "System.Threading.Tasks.Task<TResult>";

            var taskType = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1"); // 获取泛型 Task<T> 类型

            // 提取 Task<T> 的泛型类型参数 T
            ITypeSymbol innerType = returnType;

            //if (returnType.OriginalDefinition.ToString() == taskType)
            if (SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, taskType))
            {
                // 如果是 Task<T>，提取 T
                if (returnType is INamedTypeSymbol namedType && namedType.TypeArguments.Length == 1)
                {
                    innerType = namedType.TypeArguments[0];
                }
            }

            // 检查是否为 IActionResult 或其实现类

            var iActionResultType = context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.IActionResult");
#if DEBUG
            bool isIActionResultOrImplementation = innerType == iActionResultType;
            if (isIActionResultOrImplementation == false)
            {
                isIActionResultOrImplementation = innerType.ImplementsInterface(iActionResultType);
            }
#else
            bool isIActionResultOrImplementation = innerType == iActionResultType || innerType.ImplementsInterface(iActionResultType);
#endif

            if (isIActionResultOrImplementation &&
                returnStatement.Expression.IsKind(SyntaxKind.NullLiteralExpression))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, returnStatement.GetLocation(), innerType.Name));
            }
        }
    }

    public static class SymbolExtensions
    {
        public static bool ImplementsInterface(this ITypeSymbol type, INamedTypeSymbol interfaceType)
        {
            if (type == null || interfaceType == null)
            {
                return false;
            }

            if (type.TypeKind == TypeKind.Error || interfaceType.TypeKind == TypeKind.Error)
            {
                return false;
            }

            {
                // 检查实现的接口
                var result = type.AllInterfaces
                    .Where(i => i.TypeKind != TypeKind.Error)  // 排除错误类型
                    .Any(i => SymbolEqualityComparer.Default.Equals(i, interfaceType));

                if (result)
                {
                    return true;
                }
            }

            {
                var result = type.AllInterfaces.Any(i =>
                    SymbolEqualityComparer.Default.Equals(i, interfaceType) ||
                    i.ToDisplayString() == interfaceType.ToDisplayString());

                if (result)
                {
                    return true;
                }
            }

            {
                ITypeSymbol currentType = type;
                while (currentType != null && currentType.TypeKind != TypeKind.Error)
                {
                    if (currentType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, interfaceType)))
                    {
                        return true;
                    }
                    currentType = currentType.BaseType; // 继续检查父类
                }
                //return false;
            }

            return false;
        }
    }
}