namespace PerlPackageSourceGenerator.Declarations;

internal class SubRoutineDeclaration
{
    public List<string> Lines { get; }

    public SubRoutineDeclaration(SignatureDeclaration signature, IEnumerable<string> bodyLines)
    {
        Lines = new List<string>();
        Lines.AddRange(signature.Lines);
        Lines.AddRange(bodyLines);
        Lines.Add("}");
        Lines.Add(string.Empty);
    }
}
