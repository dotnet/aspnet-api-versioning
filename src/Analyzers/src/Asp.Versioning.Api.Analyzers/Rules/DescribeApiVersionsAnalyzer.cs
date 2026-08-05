// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports API version descriptions resolved before they can be complete.
/// </summary>
/// <remarks>
/// A description provider resolved from the services describes the APIs that were known when the services
/// were built. Minimal APIs are mapped onto the application afterward, so they are not among them.
/// Describing the versions from the application itself waits until every API has been mapped, which is why
/// there was nothing to choose between before minimal APIs existed.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class DescribeApiVersionsAnalyzer : DiagnosticAnalyzer
{
    private const string AddApiExplorer = nameof( AddApiExplorer );
    private const string AddODataApiExplorer = nameof( AddODataApiExplorer );
    private const string AddGrpcApiExplorer = nameof( AddGrpcApiExplorer );
    private const string AddOpenApi = nameof( AddOpenApi );
    private const string GetService = nameof( GetService );
    private const string GetRequiredService = nameof( GetRequiredService );
    private const string ApiVersioningBuilderExtensions =
        "Microsoft.Extensions.DependencyInjection.IApiVersioningBuilderExtensions";
    private const string ServiceProviderServiceExtensions =
        "Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions";
    private const string ServiceProvider = "System.IServiceProvider";
    private const string ApiVersionDescriptionProvider =
        "Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0027_UseDescribeApiVersions );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        if ( !Symbols.IsReferenced( context.Compilation, ApiVersionDescriptionProvider ) )
        {
            return;
        }

        var analysis = new Analysis();

        context.RegisterSyntaxNodeAction( analysis.OnInvocation, SyntaxKind.InvocationExpression );
        context.RegisterCompilationEndAction( analysis.OnCompilationEnd );
    }

    /// <remarks>The service can be asked for by type argument or by type, and either way through the
    /// service provider itself or through the extensions that wrap it.</remarks>
    private static bool ResolvesDescriptions(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        IMethodSymbol method,
        string declaringType )
    {
        if ( declaringType != ServiceProviderServiceExtensions && declaringType != ServiceProvider )
        {
            return false;
        }

        if ( method.TypeArguments.Length == 1 )
        {
            return method.TypeArguments[0].ToDisplayString() == ApiVersionDescriptionProvider;
        }

        var arguments = invocation.ArgumentList.Arguments;

        for ( var i = 0; i < arguments.Count; i++ )
        {
            if ( arguments[i].Expression is TypeOfExpressionSyntax typeOf &&
                 context.SemanticModel.GetTypeInfo( typeOf.Type, context.CancellationToken ).Type
                 is { } type &&
                 type.ToDisplayString() == ApiVersionDescriptionProvider )
            {
                return true;
            }
        }

        return false;
    }

    private sealed class Analysis
    {
        private readonly ConcurrentBag<Location> resolutionCallSites = [];
        private volatile bool explored;
        private volatile bool mapped;

        public void OnInvocation( SyntaxNodeAnalysisContext context )
        {
            var invocation = (InvocationExpressionSyntax) context.Node;

            if ( context.SemanticModel.GetSymbolInfo( invocation, context.CancellationToken ).Symbol
                 is not IMethodSymbol method ||
                 Symbols.ResolveDeclaringType( method ) is not { } type )
            {
                return;
            }

            var declaringType = type.ToDisplayString();

            switch ( method.Name )
            {
                case AddApiExplorer or AddODataApiExplorer or AddGrpcApiExplorer or AddOpenApi
                    when declaringType == ApiVersioningBuilderExtensions:
                    explored = true;
                    break;
                case GetService or GetRequiredService
                    when ResolvesDescriptions( context, invocation, method, declaringType ):
                    resolutionCallSites.Add( Symbols.GetLocation( invocation ) );
                    break;
                default:
                    if ( declaringType == Symbols.EndpointRouteBuilderExtensions &&
                         Endpoints.IsMapped( method.Name ) )
                    {
                        mapped = true;
                    }

                    break;
            }
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            // without a minimal API there is nothing the services were built too early to know about
            if ( !explored || !mapped || resolutionCallSites.IsEmpty )
            {
                return;
            }

            foreach ( var callSite in resolutionCallSites )
            {
                context.ReportDiagnostic( Diagnostic.Create( AV0027_UseDescribeApiVersions, callSite ) );
            }
        }
    }
}