using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerlPackageSourceGenerator.Declarations;
using PerlPackageSourceGenerator.Models;
using PerlPackageSourceGenerator.Options;

namespace PerlPackageSourceGenerator.Compilers;

internal interface ISignatureCompiler
{
    SignatureDeclaration Compile(
        CompilerOption option,
        bool isClassMethod,
        bool isPublic,
        string name,
        TypeSyntax returnType,
        IEnumerable<Parameter> parameters
    );
}
