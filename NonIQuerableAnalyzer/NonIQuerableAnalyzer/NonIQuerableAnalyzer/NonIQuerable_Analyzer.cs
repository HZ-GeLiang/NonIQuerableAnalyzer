using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace NonIQuerableAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonIQuerable_Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NonIQuerableAnalyzer";

        //// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        //// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        //private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        //private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        //private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        //private const string Category = "Naming";

        //private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        //public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
              "InvalidIQueryableMethodUsage",
              "IQueryable should not use NonIQueryable methods",
              "IQueryable should not use method '{0}' marked with NonIQueryableAttribute",
              "Usage",
              DiagnosticSeverity.Error,
              isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        //此操作表示应触发分析器以检查存在冲突的代码的代码更改。 当 Visual Studio 检测到匹配注册操作的代码编辑时，它调用分析器的注册方法。
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);

            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        //当分析器检测到冲突，它会创建一个诊断对象，Visual Studio 使用该对象来通知用户有关冲突的信息。
        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;
            var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpr == null)
            {
                return;
            }

            // var methodName = memberAccessExpr.Name.Identifier.Text;

            var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccessExpr);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
            if (methodSymbol == null || !methodSymbol.IsExtensionMethod)
            {
                return; //不是静态方法
            }

            // Check if the method has HasNonIQueryableAttribute
            if (HasNonIQueryableAttribute(methodSymbol) == false)
            {
                return; //没有特性标记的
            }

            // Get the type of the instance (the first argument)
            var receiverType = context.SemanticModel.GetTypeInfo(memberAccessExpr.Expression).Type;

            // Check if the instance type is IQueryable<>
            if (IsIQueryable(receiverType))
            {
                //针对 IEnumerableUnitTest
                var diagnostic = Diagnostic.Create(Rule, invocationExpr.GetLocation(), methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }

            #region 针对 StringUnitTest

            //var parent_receiverType = GetRootReceiverType(memberAccessExpr.Expression, context.SemanticModel);//NamedType Program.Stu

            // 获取调用链中的父级InvocationExpression（即 .Where 之类的方法）
            var parentInvocationExpr = GetParentInvocation(invocationExpr);
            if (parentInvocationExpr != null)
            {
                // 获取该父级的对象类型
                var parentType = GetParentInvocationType(parentInvocationExpr, context.SemanticModel);
                // 检查对象类型是否为 IQueryable<T>
                if (IsIQueryable(parentType))
                {
                    var diagnostic = Diagnostic.Create(Rule, invocationExpr.GetLocation(), methodSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            #endregion
        }

        // 向上查找，获取父级的 InvocationExpressionSyntax
        private InvocationExpressionSyntax GetParentInvocation(SyntaxNode node)
        {
            var current = node.Parent;
            while (current != null && !(current is InvocationExpressionSyntax))
            {
                current = current.Parent;
            }
            return current as InvocationExpressionSyntax;
        }

        private ITypeSymbol GetParentInvocationType(InvocationExpressionSyntax invocationExpr, SemanticModel semanticModel)
        {
            var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
            if (memberAccessExpr == null)
            {
                return null;
            }

            var whereObjectExpr = memberAccessExpr.Expression;
            return semanticModel.GetTypeInfo(whereObjectExpr).Type;
        }

        // 获取最上层的接收者类型 (链式调用中的最开始的对象类型) ,GPT给的方法,没验证过
        private ITypeSymbol GetRootReceiverType(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var currentExpr = expression;

            while (currentExpr is MemberAccessExpressionSyntax memberAccess)
            {
                currentExpr = memberAccess.Expression;
            }
            var type = semanticModel.GetTypeInfo(currentExpr).Type;
            return type;
        }

        private static bool HasNonIQueryableAttribute(IMethodSymbol methodSymbol)
        {
            // Check if the method or its containing class has NonIQueryableAttribute

            //方法自身
            foreach (var a in methodSymbol.GetAttributes())
            {
                var str = a.AttributeClass.Name;
                if (str.EndsWith("NonIQueryable") || str.EndsWith("NonIQueryableAttribute"))
                {
                    return true;
                }
            }

            //方法所在的类
            foreach (var a in methodSymbol.ContainingType.GetAttributes())
            {
                var str = a.AttributeClass.Name;
                if (str.EndsWith("NonIQueryable") || str.EndsWith("NonIQueryableAttribute"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsIEnumerable(ITypeSymbol type)
        {
            return type.OriginalDefinition.ToString() == "System.Collections.Generic.IEnumerable`1";
        }

        private bool IsIQueryable(ITypeSymbol type)
        {
            if (type == null)
            {
                return false;
            }

            var typeStr = type.OriginalDefinition.ToString();
            // Check if the type itself is IQueryable<>
            if (string.Equals(typeStr, "System.Linq.IQueryable<T>", System.StringComparison.Ordinal) == true ||
                string.Equals(typeStr, "SqlSugar.ISugarQueryable<T>", System.StringComparison.Ordinal) == true
                )
            {
                //如果是 System.Linq.IQueryable<T> 对象调用了某个方法
                //query.OrderByExpression(1);
                return true;
            }

            //if (type.OriginalDefinition.ToString() == "string")
            //{
            //    //a.Name.IsEqualAnyIgnoreCase("abc")
            //    return false;
            //}

            //如果 当前方法的parent 也是 System.Linq.IQueryable<T>

            /*
            // Check if any of the implemented interfaces is IQueryable<>
            foreach (var iface in type.AllInterfaces)
            {
                if (iface.OriginalDefinition.ToString() == "System.Linq.IQueryable")
                {
                    return true;
                }
            }
            */
            return false;
        }
    }
}