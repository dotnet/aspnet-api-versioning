// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports a default API version assumed where none can apply.
/// </summary>
/// <remarks>
/// A default version is only ever applied to an endpoint carrying no versioning metadata at all, which
/// grandfathers the clients of a service that was not versioned before. Declaring any version, even a
/// neutral one, takes an endpoint out of that arrangement. The setting therefore does nothing once
/// every endpoint either declares a version or can only be reached by naming one in the URL.
/// <para>Reading the version from the media type is the exception. A client asking for
/// <c>application/json</c> has named no version and never will, whereas every version after the first is
/// asked for as something like <c>application/json; v=2.0</c>. Assuming a default is what keeps the
/// original clients working, so it is left alone however the endpoints are declared.</para>
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class AssumeDefaultApiVersionAnalyzer : DiagnosticAnalyzer
{
    private const string AssumeDefaultVersionWhenUnspecified = nameof( AssumeDefaultVersionWhenUnspecified );
    private const string RouteConstraintName = nameof( RouteConstraintName );
    private const string ApiVersionReader = nameof( ApiVersionReader );
    private const string Combine = nameof( Combine );
    private const string Conventions = nameof( Conventions );
    private const string Add = nameof( Add );
    private const string VersionByNamespaceConvention = "Asp.Versioning.Conventions.VersionByNamespaceConvention";
    private const string IsApiVersionNeutral = nameof( IsApiVersionNeutral );
    private const string ApiVersionReaderType = "Asp.Versioning.ApiVersionReader";
    private const string MediaTypeApiVersionReader = "Asp.Versioning.MediaTypeApiVersionReader";
    private const string MediaTypeApiVersionReaderBuilder = "Asp.Versioning.MediaTypeApiVersionReaderBuilder";

    private static readonly HashSet<string> VersioningCalls = new( StringComparer.Ordinal )
    {
        "HasApiVersion", "HasDeprecatedApiVersion",
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0016_DoNotAssumeDefaultApiVersion );

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

    /// <summary>
    /// Represents what an API version is read from.
    /// </summary>
    private enum Reader
    {
        /// <summary>The version is read from somewhere a client must name it.</summary>
        Named,

        /// <summary>The version is read from the media type, which a client can leave unsaid.</summary>
        MediaType,

        /// <summary>The version is read from something that cannot be decided as it is written.</summary>
        Undecided,
    }

    /// <remarks>A reader combined from others reads the version from the media type when any one of them
    /// does, and a reader that cannot be read as written may well be one of them.</remarks>
    private static Reader ResolveReader( SyntaxNodeAnalysisContext context, ExpressionSyntax expression )
    {
        var model = context.SemanticModel;
        var cancellationToken = context.CancellationToken;

        if ( expression is BaseObjectCreationExpressionSyntax creation )
        {
            return Symbols.Declares(
                model.GetTypeInfo( creation, cancellationToken ).Type as INamedTypeSymbol,
                MediaTypeApiVersionReader )
                ? Reader.MediaType
                : Reader.Named;
        }

        if ( expression is not InvocationExpressionSyntax invocation ||
             model.GetSymbolInfo( invocation, cancellationToken ).Symbol is not IMethodSymbol method ||
             Symbols.ResolveDeclaringType( method ) is not { } declaringType )
        {
            return Reader.Undecided;
        }

        // a reader built from media type parameters is one whatever the built type is called
        if ( Symbols.Declares( declaringType, MediaTypeApiVersionReaderBuilder ) )
        {
            return Reader.MediaType;
        }

        if ( method.Name != Combine || declaringType.ToDisplayString() != ApiVersionReaderType )
        {
            return Reader.Undecided;
        }

        var arguments = invocation.ArgumentList.Arguments;
        var combined = Reader.Named;

        for ( var i = 0; i < arguments.Count; i++ )
        {
            switch ( ResolveReader( context, arguments[i].Expression ) )
            {
                case Reader.MediaType:
                    return Reader.MediaType;
                case Reader.Undecided:
                    combined = Reader.Undecided;
                    break;
            }
        }

        return combined;
    }

    private sealed class Analysis
    {
        private readonly ConcurrentBag<Location> assumeDefaultSites = [];
        private readonly ConcurrentBag<Endpoint> endpoints = [];
        private readonly ConcurrentBag<string> constraintNames = [];
        private volatile bool versionByNamespace;
        private volatile bool mediaType;
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

            var constant = context.SemanticModel.GetConstantValue( assignment.Right, context.CancellationToken );

            switch ( property.Name )
            {
                case AssumeDefaultVersionWhenUnspecified:
                    if ( constant is { HasValue: true, Value: true } )
                    {
                        assumeDefaultSites.Add( assignment.GetLocation() );
                    }
                    else
                    {
                        // assigned away from, or assigned something that cannot be evaluated
                        unknown = true;
                    }

                    break;
                case RouteConstraintName:
                    if ( constant is { HasValue: true, Value: string name } )
                    {
                        constraintNames.Add( name );
                    }
                    else
                    {
                        unknown = true;
                    }

                    break;
                case ApiVersionReader:
                    switch ( ResolveReader( context, assignment.Right ) )
                    {
                        case Reader.MediaType:
                            mediaType = true;
                            break;
                        case Reader.Undecided:
                            unknown = true;
                            break;
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

            if ( method.Name == Add && IsConventions( context, invocation ) )
            {
                OnConvention( context, invocation );
                return;
            }

            if ( type.ToDisplayString() == Symbols.EndpointRouteBuilderExtensions &&
                 Endpoints.IsMapped( method.Name ) )
            {
                AddEndpoint( context, invocation, method );
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
                endpoints.Add( endpoint );
            }
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            // a client that names no version is asking for the first one, whatever the endpoints declare
            if ( unknown || mediaType || assumeDefaultSites.IsEmpty || endpoints.IsEmpty )
            {
                return;
            }

            if ( !Endpoints.TryResolveConstraintName( constraintNames, out var constraintName ) )
            {
                return;
            }

            foreach ( var endpoint in endpoints )
            {
                var constrained = 0;
                var unconstrained = 0;

                foreach ( var template in endpoint.Templates )
                {
                    if ( RouteTemplate.HasConstraint( template, constraintName ) )
                    {
                        constrained++;
                    }
                    else
                    {
                        unconstrained++;
                    }
                }

                // registering the same endpoint with and without the constraint is the one way a
                // default version can be applied to a URL segment, so it is a deliberate arrangement
                if ( constrained > 0 && unconstrained > 0 )
                {
                    return;
                }

                // an endpoint declaring nothing and reachable without naming a version is exactly
                // what the default was meant for
                var versioned = endpoint.Declared ||
                                ( versionByNamespace && NamespaceVersion.IsVersioned( endpoint.Namespace ) );

                if ( !versioned && unconstrained > 0 )
                {
                    return;
                }
            }

            foreach ( var site in assumeDefaultSites )
            {
                context.ReportDiagnostic( Diagnostic.Create( AV0016_DoNotAssumeDefaultApiVersion, site ) );
            }
        }

        /// <remarks>Versioning by namespace is understood, so a controller declared in a versioned
        /// namespace is versioned by it. Any other convention may version anything at all, which
        /// leaves nothing that can be concluded about the endpoints.</remarks>
        private void OnConvention( SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation )
        {
            var arguments = invocation.ArgumentList.Arguments;

            if ( arguments.Count != 1 ||
                 context.SemanticModel.GetTypeInfo( arguments[0].Expression, context.CancellationToken ).Type
                 is not { } type )
            {
                unknown = true;
                return;
            }

            if ( type.ToDisplayString() == VersionByNamespaceConvention )
            {
                versionByNamespace = true;
            }
            else
            {
                unknown = true;
            }
        }

        private static bool IsConventions( SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation ) =>
            Routes.Receiver( invocation ) is { } receiver &&
            context.SemanticModel.GetSymbolInfo( receiver, context.CancellationToken ).Symbol
            is IPropertySymbol { Name: Conventions } property &&
            property.ContainingType?.ToDisplayString() == Symbols.MvcApiVersioningOptions;

        private void AddEndpoint(
            SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax invocation,
            IMethodSymbol method )
        {
            var applied = new HashSet<string>( StringComparer.Ordinal );

            Routes.CollectChainedCalls( invocation, applied );

            if ( Routes.GetArgument( context, invocation, method, "pattern" ) is not { } pattern )
            {
                unknown = true;
                return;
            }

            var prefix = Routes.ResolveChain( context, Routes.Receiver( invocation ), applied, out var complete );

            if ( !complete )
            {
                // a prefix that could not be followed may have carried the constraint
                unknown = true;
                return;
            }

            var versioned = applied.Overlaps( VersioningCalls );
            var neutral = applied.Contains( IsApiVersionNeutral );

            endpoints.Add( new( [prefix + "/" + pattern], versioned, neutral ) );
        }
    }
}