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
    public class EnumEqualsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NonEnum"; //EnumEquals
        private static readonly LocalizableString Title = "Avoid using .Equals for enum comparison";
        private static readonly LocalizableString MessageFormat = "Do not use .Equals for enum comparison, use '==' instead";
        private static readonly LocalizableString Description = "Avoid using .Equals() method for comparing enum values; prefer using '==' for better accuracy.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;
            var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;

            // Check if the invoked method is ".Equals"
            if (memberAccessExpr == null || memberAccessExpr.Name.Identifier.Text != "Equals")
            {
                return;
            }

            // Check if the left-hand side of ".Equals" is an enum type
            var leftHandSide = context.SemanticModel.GetTypeInfo(memberAccessExpr.Expression).Type;
            if (leftHandSide == null || !leftHandSide.TypeKind.Equals(TypeKind.Enum))
            {
                return;
            }

            // Report a diagnostic warning
            var diagnostic = Diagnostic.Create(Rule, memberAccessExpr.Name.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}