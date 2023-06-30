using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PerlPackageSourceGenerator.Models;

internal class Parameter
{
    public string Name { get; }
    public string Type { get; }
    public TypeSyntax? OriginalCSharpType { get; }

    public Parameter(string name, string type, TypeSyntax? originalCSharpType = null)
    {
        Name = name;
        Type = type;
        OriginalCSharpType = originalCSharpType;
    }
}
