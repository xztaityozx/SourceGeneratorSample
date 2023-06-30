﻿using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using PerlPackageSourceGenerator.Compilers;
using PerlPackageSourceGenerator.Options;

namespace PerlPackageSourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class PerlPackageGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // このジェネレーターの対象にするクラスへ付与する属性をSourceGeneratorで生成してるところ
        const string attributeClassName = "PackageGeneratorAttribute";
        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.AddSource(
                $"{attributeClassName}.cs",
                $$"""
namespace PerlPackageSourceGenerator;

[global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class {{attributeClassName}} : global::System.Attribute {}

"""
            );
        });

        var source = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                $"PerlPackageSourceGenerator.{attributeClassName}",
                static (node, token) => true,
                static (context, token) => context
            )
            .Collect();

        var option = context.AnalyzerConfigOptionsProvider.Select(Options.Create).Combine(source);

        context.RegisterSourceOutput(option, Generate);
    }

    private static void Generate(
        SourceProductionContext context,
        (Options Left, ImmutableArray<GeneratorAttributeSyntaxContext> Right) source
    )
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var syntaxContext = source.Right.FirstOrDefault();

        var typeSymbol = (INamedTypeSymbol)syntaxContext.TargetSymbol;
        var typeNode = (ClassDeclarationSyntax)syntaxContext.TargetNode;

        var classCompiler = new ClassCompiler(
            new CompilerOption(),
            syntaxContext.SemanticModel,
            context.ReportDiagnostic
        );

        var @namespace = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? ""
            : $"namespace {typeSymbol.ContainingNamespace};";

        var className = typeSymbol
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "");
        var segments = className.Replace("<", "_").Replace(">", "_").Split('.');
        var classDeclaration = classCompiler.Compile(typeNode, className);

        var code = $$"""
// <auto-generated />
// This source code was generated by a PerlPackageSourceGenerator.
#nullable enable

{{@namespace}}

partial class {{typeSymbol.Name}}
{

    public async global::System.Threading.Tasks.Task PerlPackageSourceGenerator_GeneratePerlAsync(string baseDir) 
    {
        var base64EncodedCode = @"{{Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join("\n", classDeclaration.Lines)))}}";
        var code = global::System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedCode));
        var path = global::System.IO.Path.Combine(
            baseDir,
            "Generated",
            "Perl",
            {{string.Join(", ", segments.Select(s => $"\"{s}\""))}}
        ) + ".g.pm";

        var dir = global::System.IO.Path.GetDirectoryName(path);
        if(dir is not null && !global::System.IO.Directory.Exists(dir)) {
            global::System.IO.Directory.CreateDirectory(dir);
        }

        await using var stream = global::System.IO.File.OpenWrite(path);
        await using var writer = new global::System.IO.StreamWriter(stream, global::System.Text.Encoding.UTF8);
        await writer.WriteAsync(code);
        await writer.FlushAsync();
    }
}
""";
        context.AddSource($"{string.Join(".", segments)}.PerlPackageGenerator.g.cs", code);
    }

    /// <summary>
    /// ビルドプロパティからプロジェクトルートを取り出して格納しておくためのクラス
    /// </summary>
    internal sealed class Options
    {
        public readonly string ProjectDirectory;

        public Options(string projectDirectory) => ProjectDirectory = projectDirectory;

        /// <summary>
        /// ただのファクトリメソッド。AnalyzerConfigOptionsProvider.Select()に渡せば良い
        /// </summary>
        /// <param name="optionsProvider"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Options Create(
            AnalyzerConfigOptionsProvider optionsProvider,
            CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var options = new Options(
                optionsProvider.GlobalOptions.TryGetValue(
                    "build_property.ProjectDir",
                    out var projectDir
                )
                    ? projectDir
                    // 無かったら例外投げてるけど、ないことなんてある？
                    : throw new InvalidOperationException("ProjectDir is not defined")
            );

            return options;
        }
    }
}
