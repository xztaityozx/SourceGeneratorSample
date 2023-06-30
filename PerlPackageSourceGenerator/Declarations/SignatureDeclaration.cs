using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerlPackageSourceGenerator.Extensions;
using PerlPackageSourceGenerator.Models;

namespace PerlPackageSourceGenerator.Declarations;

internal class SignatureDeclaration
{
    public SignatureDeclaration(
        string name,
        TypeSyntax returnType,
        IEnumerable<Parameter> parameters
    )
    {
        var @params = parameters.ToArray();
        var originalSignature =
            returnType
            + " "
            + name
            + "("
            + string.Join(
                ", ",
                @params
                    .Where(p => p.OriginalCSharpType is not null)
                    .Select(p => $"{p.OriginalCSharpType} {p.Name.TrimStart('$')}")
            )
            + ")";

        var lines = new List<string>
        {
            $"# Original Method Name: {originalSignature}",
            "#@returns " + returnType.ToPerlType(),
            $"sub {name.ToSnakeCase()} {{",
            "    my (",
            $"        {string.Join(", ", @params.Select(p => p.Name.StartsWith("$") ? p.Name.ToSnakeCase() : $"${p.Name.ToSnakeCase()}"))}",
            "    ) = @_;",
        };

        Lines = lines;
    }

    public IEnumerable<string> Lines { get; }
}
