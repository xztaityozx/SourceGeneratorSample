﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace HelloWorldSourceGenerator;

[Generator(LanguageNames.CSharp)]
public class HelloWorldGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        const string attributeClassName = "HelloWorldAttribute";
        context.RegisterPostInitializationOutput(static ctx => {
            ctx.AddSource(
                $"{attributeClassName}.cs",
                $$"""
namespace HelloWorldSourceGenerator;

[global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class {{attributeClassName}} : global::System.Attribute {}
""");
        });

        var source = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                $"HelloWorldSourceGenerator.{attributeClassName}",
                static (node, token) => true,
                static (context, token) => context
            )
            .Collect();
        
        context.RegisterSourceOutput(source, Generate);
    }

    private static void Generate(
        SourceProductionContext context,
        ImmutableArray<GeneratorAttributeSyntaxContext> source
    ) {
        context.CancellationToken.ThrowIfCancellationRequested();

        foreach (var syntaxContext in source) {
            var typeSymbol = (INamedTypeSymbol)syntaxContext.TargetSymbol;

            var @namespace = typeSymbol.ContainingNamespace.IsGlobalNamespace
                ? ""
                : $"namespace {typeSymbol.ContainingNamespace.ToDisplayString()};";

            var fileName = string.Join(".", typeSymbol
                .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                .Replace("global::", "")
                .Replace("<", "_")
                .Replace(">", "_")
                .Split(".")) + ".g.cs";
            
            var code = $$"""
// <auto-generated />

{{@namespace}}

partial class {{typeSymbol.Name}}
{
    public void Hello()
    {
        global::System.Console.WriteLine("Hello, World!");
    }
}

""";

            context.AddSource(fileName, code);
        }
    }
}