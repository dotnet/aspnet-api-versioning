// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports an API reading versions one way without having said so.
/// </summary>
/// <remarks>
/// Without an explicit reader, a version is looked for in both the query string and the URL segment.
/// Every route is examined to decide which of the two is actually used. A route that cannot be
/// followed to its origin, and any mixture of the two styles, leaves the default in place, because
/// narrowing the reader would then break a form the application relies on.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class SpecificApiVersionReaderAnalyzer : DiagnosticAnalyzer
{
    private const string UrlSegmentApiVersionReader = nameof( UrlSegmentApiVersionReader );
    private const string QueryStringApiVersionReader = nameof( QueryStringApiVersionReader );
    private const string ApiVersionReader = nameof( ApiVersionReader );
    private const string RouteConstraintName = nameof( RouteConstraintName );
    private const string AddApiVersioning = nameof( AddApiVersioning );
    private const string AddRouteComponents = nameof( AddRouteComponents );
    private const string IsApiVersionNeutral = nameof( IsApiVersionNeutral );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0015_UseSpecificApiVersionReader );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        var analysis = new Analysis();

        context.RegisterSyntaxNodeAction( analysis.OnAssignment, SyntaxKind.SimpleAssignmentExpression );
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
        private readonly ConcurrentBag<Route> routes = [];
        private readonly ConcurrentBag<string> constraintNames = [];
        private volatile bool readerConfigured;
        private volatile bool unknown;

        public void OnAssignment( SyntaxNodeAnalysisContext context )
        {
            var assignment = (AssignmentExpressionSyntax) context.Node;

            if ( context.SemanticModel.GetSymbolInfo( assignment.Left, context.CancellationToken ).Symbol
                 is not IPropertySymbol property ||
                 property.ContainingType?.ToDisplayString() != Symbols.ApiVersioningOptions )
            {
                return;
            }

            switch ( property.Name )
            {
                case ApiVersionReader:
                    readerConfigured = true;
                    break;
                case RouteConstraintName:
                    var constant = context.SemanticModel.GetConstantValue( assignment.Right, context.CancellationToken );

                    if ( constant is { HasValue: true, Value: string name } )
                    {
                        constraintNames.Add( name );
                    }
                    else
                    {
                        // a constraint cannot be recognized in a template without knowing its name
                        unknown = true;
                    }

                    break;
            }
        }

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
            else if ( method.Name == AddRouteComponents && declaringType == Symbols.ODataApiVersioningOptions )
            {
                AddODataRoute( context, invocation, method );
            }
            else if ( declaringType == Symbols.EndpointRouteBuilderExtensions && Endpoints.IsMapped( method.Name ) )
            {
                AddEndpointRoute( context, invocation, method );
            }
        }

        public void OnNamedType( SymbolAnalysisContext context )
        {
            var type = (INamedTypeSymbol) context.Symbol;

            if ( !Symbols.IsApiController( type ) ||
                 Symbols.HasAttribute( type, Symbols.ApiVersionNeutralAttribute ) )
            {
                return;
            }

            foreach ( var endpoint in Endpoints.FromController( type ) )
            {
                foreach ( var template in endpoint.Templates )
                {
                    routes.Add( new( template, complete: true ) );
                }
            }
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            if ( readerConfigured || unknown || apiVersioningCallSites.IsEmpty || routes.IsEmpty )
            {
                return;
            }

            if ( !Endpoints.TryResolveConstraintName( constraintNames, out var constraintName ) )
            {
                return;
            }

            var urlSegment = false;
            var queryString = false;

            foreach ( var route in routes )
            {
                if ( RouteTemplate.HasConstraint( route.Template, constraintName ) )
                {
                    urlSegment = true;
                }
                else if ( route.Complete )
                {
                    queryString = true;
                }
                else
                {
                    // a prefix that could not be followed may have carried the constraint
                    return;
                }

                // the first mixture of the two styles is enough to leave the default alone
                if ( urlSegment && queryString )
                {
                    return;
                }
            }

            var reader = urlSegment ? UrlSegmentApiVersionReader : QueryStringApiVersionReader;

            foreach ( var callSite in apiVersioningCallSites )
            {
                context.ReportDiagnostic( Diagnostic.Create( AV0015_UseSpecificApiVersionReader, callSite, reader ) );
            }
        }

        private void AddODataRoute(
            SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax invocation,
            IMethodSymbol method )
        {
            // the prefix applies to every OData controller registered with it
            if ( method.Parameters.Length == 0 || method.Parameters[0].Type.SpecialType != SpecialType.System_String )
            {
                routes.Add( new( string.Empty, complete: true ) );
            }
            else if ( Routes.GetArgument( context, invocation, method, "prefix" ) is { } prefix )
            {
                routes.Add( new( prefix, complete: true ) );
            }
            else
            {
                unknown = true;
            }
        }

        private void AddEndpointRoute(
            SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax invocation,
            IMethodSymbol method )
        {
            var applied = new HashSet<string>( StringComparer.Ordinal );

            Routes.CollectChainedCalls( invocation, applied );

            if ( applied.Contains( IsApiVersionNeutral ) )
            {
                return;
            }

            if ( Routes.GetArgument( context, invocation, method, "pattern" ) is not { } pattern )
            {
                unknown = true;
                return;
            }

            var prefix = Routes.ResolveChain( context, Routes.Receiver( invocation ), applied, out var complete );

            if ( applied.Contains( IsApiVersionNeutral ) )
            {
                return;
            }

            routes.Add( new( prefix + "/" + pattern, complete ) );
        }
    }
}