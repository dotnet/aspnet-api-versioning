// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports OData route components which are never applied.
/// </summary>
/// <remarks>
/// Versioned OData resolves the options for the API version of the current request, which the options
/// configured for OData itself are not part of. Route components added without saying which API version
/// they belong to are left behind when the options are resolved, and a prefix stated in both places
/// collides once they are. Neither is a supported way to reach a versioned OData API.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class IgnoredRouteComponentsAnalyzer : DiagnosticAnalyzer
{
    private const string AddOData = nameof( AddOData );
    private const string AddRouteComponents = nameof( AddRouteComponents );
    private const string ODataOptions = "Microsoft.AspNetCore.OData.ODataOptions";
    private const string ApiVersioningBuilderExtensions =
        "Microsoft.Extensions.DependencyInjection.IApiVersioningBuilderExtensions";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0023_IgnoredRouteComponents );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        // the options that carry route components cannot be configured without the library that declares them
        if ( !Symbols.IsReferenced( context.Compilation, ODataOptions ) )
        {
            return;
        }

        var analysis = new Analysis();

        context.RegisterSyntaxNodeAction( analysis.OnInvocation, SyntaxKind.InvocationExpression );
        context.RegisterCompilationEndAction( analysis.OnCompilationEnd );
    }

    private sealed class Analysis
    {
        private readonly ConcurrentBag<Location> routeComponentCallSites = [];
        private volatile bool versionsOData;

        public void OnInvocation( SyntaxNodeAnalysisContext context )
        {
            var invocation = (InvocationExpressionSyntax) context.Node;

            if ( context.SemanticModel.GetSymbolInfo( invocation, context.CancellationToken ).Symbol
                 is not IMethodSymbol method ||
                 Symbols.ResolveDeclaringType( method ) is not { } type )
            {
                return;
            }

            // the versioned options declare AddRouteComponents of their own, which is the correct one
            var declaringType = type.ToDisplayString();

            switch ( method.Name )
            {
                case AddOData when declaringType == ApiVersioningBuilderExtensions:
                    versionsOData = true;
                    break;
                case AddRouteComponents when declaringType == ODataOptions:
                    routeComponentCallSites.Add( Symbols.GetLocation( invocation ) );
                    break;
            }
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            // route components are applied as they are written until versioned OData replaces the options
            if ( !versionsOData || routeComponentCallSites.IsEmpty )
            {
                return;
            }

            foreach ( var callSite in routeComponentCallSites )
            {
                context.ReportDiagnostic( Diagnostic.Create( AV0023_IgnoredRouteComponents, callSite ) );
            }
        }
    }
}