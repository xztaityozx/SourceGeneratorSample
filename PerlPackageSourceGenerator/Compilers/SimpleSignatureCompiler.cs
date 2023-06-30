using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerlPackageSourceGenerator.Declarations;
using PerlPackageSourceGenerator.Models;
using PerlPackageSourceGenerator.Options;

namespace PerlPackageSourceGenerator.Compilers;

internal class SimpleSignatureCompiler : ISignatureCompiler
{
    public SignatureDeclaration Compile(
        CompilerOption option,
        bool isClassMethod,
        bool isPublic,
        string name,
        TypeSyntax returnType,
        IEnumerable<Parameter> parameters
    )
    {
        if (isClassMethod)
        {
            parameters = parameters.Prepend(
                new Parameter(option.InstanceVariableName, "__PACKAGE__")
            );
        }

        return new SignatureDeclaration(name, returnType, parameters);
    }
}
