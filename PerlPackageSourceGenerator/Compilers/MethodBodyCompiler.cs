using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerlPackageSourceGenerator.Options;

namespace PerlPackageSourceGenerator.Compilers;

internal class MethodBodyCompiler
{
    private readonly SemanticModel semanticModel;
    private readonly Action<Diagnostic> diagnosticReporter;
    private readonly CompilerOption option;

    public MethodBodyCompiler(
        CompilerOption option,
        Action<Diagnostic> diagnosticReporter,
        SemanticModel semanticModel
    )
    {
        this.semanticModel = semanticModel;
        this.diagnosticReporter = diagnosticReporter;
        this.option = option;
    }

    public IEnumerable<string> Compile(MethodDeclarationSyntax methodDeclarationSyntax)
    {
        var bodies =
            methodDeclarationSyntax.Body?.Statements.Select(ConvertStatement)
            ?? Enumerable.Empty<string>();
        return bodies.Select(b => $"    {b}");
    }

    private string ConvertStatement(StatementSyntax statement)
    {
        if (statement is ReturnStatementSyntax returnStatement)
        {
            return $"return {ConvertExpression(returnStatement.Expression)};";
        }

        if (statement is ThrowStatementSyntax throwStatement)
        {
            return "die(\"" + throwStatement.Expression + "\");";
        }

        // むずすぎる…
        if (statement is SwitchStatementSyntax switchStatement)
        {
            throw new SwitchStatementIsNotSupportedException(switchStatement);
        }

        if (statement is ExpressionStatementSyntax expressionStatement)
        {
            return ConvertExpression(expressionStatement.Expression);
        }

        Debug.WriteLine(statement);
        return $"# {statement}";
    }

    private string ConvertExpression(ExpressionSyntax? expression)
    {
        if (expression is null)
            return "";

        if (expression is IdentifierNameSyntax identifierName)
        {
            var symbol = semanticModel
                .LookupSymbols(identifierName.SpanStart, name: identifierName.Identifier.ValueText)
                .FirstOrDefault();

            if (symbol is null)
            {
                diagnosticReporter.Invoke(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "PERLSG0001",
                            "Unknown Symbol",
                            "解決できないシンボルでした。グローバルな関数としてトランスパイルされます。{0}",
                            "PerlPackageSourceGenerator",
                            DiagnosticSeverity.Warning,
                            true
                        ),
                        identifierName.GetLocation(),
                        identifierName.Identifier.ValueText
                    )
                );
                return $"{identifierName.Identifier.ValueText}";
            }

            if (symbol.Kind == SymbolKind.Parameter)
            {
                return $"${identifierName.Identifier.ValueText}";
            }

            // Propertyは、インスタンス変数として扱う...のはおかしいんだけど
            // getter/setterを作るためのライブラリを選定してないとか、依存ライブラリを増やしたくないとか、
            // getterの処理、setterの処理変換するとかがありめんどくさい
            // なので今回は頑張ってない
            if (symbol.Kind == SymbolKind.Property)
            {
                return $"{option.InstanceVariableName}->{{{symbol.Name}}}";
            }

            return identifierName.Identifier.ValueText;
        }

        if (expression is BinaryExpressionSyntax binaryExpression)
        {
            var left = ConvertExpression(binaryExpression.Left);
            var right = ConvertExpression(binaryExpression.Right);
            var op = binaryExpression.OperatorToken.ValueText;
            return $"{left} {op} {right}";
        }

        Debug.WriteLine(expression);
        return $"# {expression}";
    }
}

public class SwitchStatementIsNotSupportedException : Exception
{
    public SwitchStatementIsNotSupportedException(SwitchStatementSyntax switchStatement)
        : base($"switch statement is not supported. Original statement: {switchStatement}") { }
}
