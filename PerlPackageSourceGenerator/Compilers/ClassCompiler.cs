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

        var constructorCodeLines = new List<string>
        {
            "sub new {",
            "    my ($class, $args) = @_;",
            // Todo: ここにバリデーションコードを入れる
            "    my $self = +{",
            "        %$args",
            "    };",
            "    bless $self, $class;",
            "    return $self;",
            "}",
        };

        return new ClassDeclaration(className, subroutines);
    }
}
