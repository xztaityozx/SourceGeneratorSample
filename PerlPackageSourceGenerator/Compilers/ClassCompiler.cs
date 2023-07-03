using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PerlPackageSourceGenerator.Declarations;
using PerlPackageSourceGenerator.Options;

namespace PerlPackageSourceGenerator.Compilers;

internal class ClassCompiler
{
    private readonly CompilerOption option;
    private readonly SemanticModel semanticModel;
    private readonly Action<Diagnostic> diagnosticReporter;

    public ClassCompiler(
        CompilerOption option,
        SemanticModel semanticModel,
        Action<Diagnostic> diagnosticReporter
    )
    {
        this.option = option;
        this.semanticModel = semanticModel;
        this.diagnosticReporter = diagnosticReporter;
    }

    public ClassDeclaration Compile(ClassDeclarationSyntax classDeclarationSyntax, string className)
    {
        var compilerOption = new CompilerOption();
        var methodCompiler = new MethodCompiler(compilerOption, diagnosticReporter, semanticModel);

        var subroutines = classDeclarationSyntax.Members
            .OfType<MethodDeclarationSyntax>()
            .Select(methodCompiler.Compile)
            .ToArray();

        var properties = classDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>()
            .Select(prop => prop.Identifier.ValueText)
            .ToArray();

        var fieldNameList = classDeclarationSyntax.Members.OfType<FieldDeclarationSyntax>()
            .SelectMany(field => field.Declaration.Variables)
            .Select(d => d.Identifier.ValueText);

        var constructorCodeLines = new List<string>
        {
            "sub new {",
            "    my ($class, $args) = @_;",
            "    $args //= +{};",
            "    my @fields = qw(" + string.Join(" ", properties.Concat(fieldNameList)) + ");",
            "    my $self = +{ %$args };",
            "    for my $field (@fields) {",
            "        $self->{$field} //= undef;",
            "    }",
            "    bless $self, $class;",
            "    return $self;",
            "}",
        };

        return new ClassDeclaration(className, constructorCodeLines ,subroutines);
    }
}
