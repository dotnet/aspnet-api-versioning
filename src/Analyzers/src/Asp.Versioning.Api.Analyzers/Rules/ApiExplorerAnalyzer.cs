// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports an API explorer which is unaware of API versions.
/// </summary>
/// <remarks>
/// The versioned API explorer adds the endpoints API explorer itself, so adding it alongside is
/// redundant. Adding it on its own describes endpoints without their versions, which is rarely what
/// was intended once API versioning is in use.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class ApiExplorerAnalyzer : DiagnosticAnalyzer
{
    private const string AddApiVersioning = nameof( AddApiVersioning );
    private const string AddApiExplorer = nameof( AddApiExplorer );
    private const string AddODataApiExplorer = nameof( AddODataApiExplorer );
    private const string AddOpenApi = nameof( AddOpenApi );
    private const string AddEndpointsApiExplorer = nameof( AddEndpointsApiExplorer );
    private const string ApiVersioningBuilderExtensions =
        "Microsoft.Extensions.DependencyInjection.IApiVersioningBuilderExtensions";
    private const string EndpointMetadataApiExplorerServiceCollectionExtensions =
        "Microsoft.Extensions.DependencyInjection.EndpointMetadataApiExplorerServiceCollectionExtensions";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            AV0020_UnnecessaryEndpointsApiExplorer,
            AV0021_UseVersionedApiExplorer );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        if ( !Symbols.IsReferenced( context.Compilation, EndpointMetadataApiExplorerServiceCollectionExtensions ) )
        {
            return;
        }

        var analysis = new Analysis();

        context.RegisterSyntaxNodeAction( analysis.OnInvocation, SyntaxKind.InvocationExpression );
        context.RegisterCompilationEndAction( analysis.OnCompilationEnd );
    }

    private sealed class Analysis
    {
        private readonly ConcurrentBag<Location> endpointsApiExplorerCallSites = [];
        private volatile bool versioned;
        private volatile bool versionedApiExplorer;

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
                case AddApiVersioning when declaringType == Symbols.ServiceCollectionExtensions:
                    versioned = true;
                    break;

                // the OData and OpenAPI variants add the versioned explorer on their way to their own
                case AddApiExplorer when declaringType == ApiVersioningBuilderExtensions:
                case AddODataApiExplorer when declaringType == ApiVersioningBuilderExtensions:
                case AddOpenApi when declaringType == ApiVersioningBuilderExtensions:
                    versionedApiExplorer = true;
                    break;
                case AddEndpointsApiExplorer
                    when declaringType == EndpointMetadataApiExplorerServiceCollectionExtensions:
                    endpointsApiExplorerCallSites.Add( invocation.Parent is ExpressionStatementSyntax statement
                                                       ? statement.GetLocation()
                                                       : invocation.GetLocation() );
                    break;
            }
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            if ( endpointsApiExplorerCallSites.IsEmpty )
            {
                return;
            }

            // the versioned explorer adds this one itself, so the call is redundant rather than wrong
            var descriptor = versionedApiExplorer ? AV0020_UnnecessaryEndpointsApiExplorer
                           : versioned ? AV0021_UseVersionedApiExplorer
                           : default;

            if ( descriptor is null )
            {
                return;
            }

            foreach ( var callSite in endpointsApiExplorerCallSites )
            {
                context.ReportDiagnostic( Diagnostic.Create( descriptor, callSite ) );
            }
        }
    }
}