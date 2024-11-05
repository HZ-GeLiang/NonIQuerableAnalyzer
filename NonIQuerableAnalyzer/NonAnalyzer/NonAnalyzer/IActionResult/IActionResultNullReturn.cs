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
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ReturnStatement);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var returnStatement = (ReturnStatementSyntax)context.Node;

            //// 确保诊断逻辑匹配测试代码中的返回模式
            //if (returnStatement.Expression.IsKind(SyntaxKind.NullLiteralExpression))
            //{
            //    context.ReportDiagnostic(Diagnostic.Create(DiagnosticRule, returnStatement.GetLocation()));
            //}

            // 检查返回的是否是 null
            if (returnStatement.Expression?.IsKind(SyntaxKind.NullLiteralExpression) != true)
            {
                return;
            }

            // 获取包含此 return 语句的方法声明
            var containingMethod = returnStatement.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (containingMethod == null)
            {
                return;
            }

            // 获取方法的返回类型
            var methodReturnType = context.SemanticModel.GetTypeInfo(containingMethod.ReturnType).Type;
            if (methodReturnType == null)
            {
                return;
            }

            // 获取 IActionResult 的类型引用
            var iActionResultType = context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.IActionResult");

            if (iActionResultType == null)
            {
                return;
            }

            // 检查方法的返回类型是否为 IActionResult 或其派生类型
            if (methodReturnType.Equals(iActionResultType) || methodReturnType.AllInterfaces.Contains(iActionResultType))
            {
                // 如果返回类型是 IActionResult 且返回了 null，报告诊断
                var diagnostic = Diagnostic.Create(Rule, returnStatement.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}