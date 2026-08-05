// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports an API where nothing is versioned.
/// </summary>
/// <remarks>
/// A version-neutral endpoint belongs to every API version that has been defined. Requests still route
/// when nothing else is defined, which is why this can go unnoticed, but the API explorer describes an
/// endpoint once per explicitly defined version. With none defined, it describes nothing at all.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class AllEndpointsVersionNeutralAnalyzer : DiagnosticAnalyzer
{
    private const string AddApiVersioning = nameof( AddApiVersioning );
    private const string IsApiVersionNeutral = nameof( IsApiVersionNeutral );

    private static readonly HashSet<string> VersioningCalls = new( StringComparer.Ordinal )
    {
        "HasApiVersion", "HasDeprecatedApiVersion",
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0018_AllEndpointsAreVersionNeutral );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        var analysis = new Analysis();

        context.RegisterSyntaxNodeAction( analysis.OnInvocation, SyntaxKind.InvocationExpression );

        // controllers cannot exist without MVC, so there is nothing to walk the declared types for
        if ( Symbols.IsReferenced( context.Compilation, Symbols.ControllerBase ) )
        {
            context.RegisterSymbolAction( analysis.OnNamedType, SymbolKind.NamedType );
        }

        context.RegisterCompilationEndAction( analysis.OnCompilationEnd );
    }

    private sealed class Analysis
    {
        private readonly ConcurrentBag<Location> apiVersioningCallSites = [];
        private volatile bool anyEndpoint;
        private volatile bool anyVersioned;
        private volatile bool anyUndeclared;
        private volatile bool unknown;

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

            if ( method.Name == AddApiVersioning && declaringType == Symbols.ServiceCollectionExtensions )
            {
                apiVersioningCallSites.Add( Symbols.GetLocation( invocation ) );
            }
            else if ( declaringType == Symbols.EndpointRouteBuilderExtensions && Endpoints.IsMapped( method.Name ) )
            {
                AddEndpoint( context, invocation );
            }
        }

        public void OnNamedType( SymbolAnalysisContext context )
        {
            var type = (INamedTypeSymbol) context.Symbol;

            if ( !Symbols.IsApiController( type ) )
            {
                return;
            }

            foreach ( var endpoint in Endpoints.FromController( type ) )
            {
                Add( endpoint.Versioned, endpoint.Neutral );
            }
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            // an endpoint that declares nothing is a separate problem, and a version declared
            // anywhere gives the API explorer something to describe every neutral endpoint against
            if ( unknown || !anyEndpoint || anyVersioned || anyUndeclared || apiVersioningCallSites.IsEmpty )
            {
                return;
            }

            foreach ( var callSite in apiVersioningCallSites )
            {
                context.ReportDiagnostic( Diagnostic.Create( AV0018_AllEndpointsAreVersionNeutral, callSite ) );
            }
        }

        private void Add( bool versioned, bool neutral )
        {
            anyEndpoint = true;

            if ( versioned )
            {
                anyVersioned = true;
            }

            if ( !neutral )
            {
                anyUndeclared = true;
            }
        }

        private void AddEndpoint( SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation )
        {
            var applied = new HashSet<string>( StringComparer.Ordinal );

            Routes.CollectChainedCalls( invocation, applied );
            Routes.ResolveChain( context, Routes.Receiver( invocation ), applied, out var complete );

            if ( !complete )
            {
                // a group that could not be followed may have declared a version of its own
                unknown = true;
                return;
            }

            Add( applied.Overlaps( VersioningCalls ), applied.Contains( IsApiVersionNeutral ) );
        }
    }
}