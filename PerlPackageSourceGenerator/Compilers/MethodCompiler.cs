using System.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerlPackageSourceGenerator.Declarations;
using PerlPackageSourceGenerator.Extensions;
using PerlPackageSourceGenerator.Models;
using PerlPackageSourceGenerator.Options;

namespace PerlPackageSourceGenerator.Compilers;

internal class MethodCompiler
{
    private readonly MethodBodyCompiler methodBodyCompiler;
    private readonly ISignatureCompiler signatureCompiler;
    private readonly CompilerOption option;

    public MethodCompiler(
        CompilerOption option,
        Action<Diagnostic> diagnosticReporter,
        SemanticModel semanticModel
    )
    {
        this.option = option;

        signatureCompiler = option.SignatureType switch
        {
            SignatureType.Simple => new SimpleSignatureCompiler(),
            _ => throw new NotImplementedException()
        };
        methodBodyCompiler = new MethodBodyCompiler(option, diagnosticReporter, semanticModel);
    }

    public SubRoutineDeclaration Compile(MethodDeclarationSyntax syntax)
    {
        var isPublic = syntax.Modifiers.Any(SyntaxKind.PublicKeyword);
        var isClassMethod = !syntax.Modifiers.Any(SyntaxKind.StaticKeyword);
        var name = syntax.Identifier.ValueText;
        var returnType = syntax.ReturnType;
        var parameters = syntax.ParameterList.Parameters.Select(
            p =>
                new Parameter(
                    $"${p.Identifier.ValueText}",
                    p.Type?.ToPerlType() ?? throw new NoNullAllowedException(),
                    p.Type ?? throw new NoNullAllowedException()
                )
        );

        var signature = signatureCompiler.Compile(
            option,
            isClassMethod,
            isPublic,
            name,
            returnType,
            parameters
        );

        var body = methodBodyCompiler.Compile(syntax);
        return new SubRoutineDeclaration(signature, body);
    }
}
