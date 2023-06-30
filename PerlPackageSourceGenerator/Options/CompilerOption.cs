using PerlPackageSourceGenerator.Models;

namespace PerlPackageSourceGenerator.Options;

public class CompilerOption
{
    public string InstanceVariableName { get; } = "$self";
    public SignatureType SignatureType { get; } = SignatureType.Simple;
}
