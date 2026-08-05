// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports an OpenAPI document left without the information describing it.
/// </summary>
/// <remarks>
/// What a document says about itself is taken from the assembly it is generated for, whether the attribute
/// carrying it was written by hand or generated from the project. The assembly it is taken from is the one
/// the application was started from, so a library that configures OpenAPI on the application's behalf has
/// nothing to give. The title of a document is taken the same way, but the project supplies one whether it
/// was asked for or not, so there is nothing to report about it.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class MissingDocumentInfoAnalyzer : DiagnosticAnalyzer
{
    private const string AddOpenApi = nameof( AddOpenApi );
    private const string ApiVersioningBuilderExtensions =
        "Microsoft.Extensions.DependencyInjection.IApiVersioningBuilderExtensions";
    private const string AssemblyDescriptionAttribute = "System.Reflection.AssemblyDescriptionAttribute";
    private const string VersionedOpenApiOptions = "Asp.Versioning.OpenApi.VersionedOpenApiOptions";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0025_MissingDocumentDescription );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        var compilation = context.Compilation;

        if ( !Symbols.IsReferenced( compilation, VersionedOpenApiOptions ) ||
             !IsApplication( compilation ) ||
             HasValue( compilation.Assembly, AssemblyDescriptionAttribute ) )
        {
            return;
        }

        context.RegisterSyntaxNodeAction( OnInvocation, SyntaxKind.InvocationExpression );
    }

    /// <remarks>The document is described from the assembly the application was started from, which is
    /// only the assembly being compiled when that assembly is the application itself.</remarks>
    private static bool IsApplication( Compilation compilation ) =>
        compilation.Options.OutputKind is OutputKind.ConsoleApplication or OutputKind.WindowsApplication;

    private static bool HasValue( IAssemblySymbol assembly, string attributeName )
    {
        foreach ( var attribute in assembly.GetAttributes() )
        {
            if ( attribute.AttributeClass?.ToDisplayString() != attributeName )
            {
                continue;
            }

            // a value that is empty is left out of the document the same way a missing one is
            return attribute.ConstructorArguments.Length == 1 &&
                   attribute.ConstructorArguments[0].Value is string value &&
                   !string.IsNullOrEmpty( value );
        }

        return false;
    }

    private static void OnInvocation( SyntaxNodeAnalysisContext context )
    {
        var invocation = (InvocationExpressionSyntax) context.Node;

        if ( context.SemanticModel.GetSymbolInfo( invocation, context.CancellationToken ).Symbol
             is not IMethodSymbol { Name: AddOpenApi } method ||
             Symbols.ResolveDeclaringType( method )?.ToDisplayString() != ApiVersioningBuilderExtensions )
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create( AV0025_MissingDocumentDescription, Symbols.GetLocation( invocation ) ) );
    }
}