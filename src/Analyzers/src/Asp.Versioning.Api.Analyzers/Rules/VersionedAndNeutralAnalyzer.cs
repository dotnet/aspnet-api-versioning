// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports an API declared both versioned and version-neutral.
/// </summary>
/// <remarks>
/// Versioning metadata is inherited from a controller or an endpoint group as a convenience, and an
/// action may state something more explicit in its place. The exception is neutrality, which applies to
/// the whole API; an action cannot meaningfully claim a version of an API that has none. Controllers
/// are collated by logical name, so a neutral declaration on one can silence versions declared on
/// another that collates alongside it.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class VersionedAndNeutralAnalyzer : DiagnosticAnalyzer
{
    private const string IsApiVersionNeutral = nameof( IsApiVersionNeutral );
    private const string ControllerNameConvention = "Asp.Versioning.Conventions.ControllerNameConvention";
    private const string ControllerNameConventionOf = "Asp.Versioning.Conventions.IControllerNameConvention";

    private static readonly HashSet<string> VersioningCalls = new( StringComparer.Ordinal )
    {
        "HasApiVersion", "HasDeprecatedApiVersion",
    };

    /// <remarks>Collation applies GroupName over NormalizeName, and only these two trim trailing
    /// numbers between them. Any other convention collates by rules this cannot reproduce.</remarks>
    private static readonly HashSet<string> TrimmingConventions = new( StringComparer.Ordinal )
    {
        "Asp.Versioning.Conventions.DefaultControllerNameConvention",
        "Asp.Versioning.Conventions.GroupedControllerNameConvention",
        "Default",
        "Grouped",
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0019_VersionedAndNeutral );

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

    private static Location? GetLocation( ISymbol symbol, string attributeName )
    {
        foreach ( var attribute in symbol.GetAttributes() )
        {
            if ( attribute.AttributeClass?.ToDisplayString() == attributeName &&
                 attribute.ApplicationSyntaxReference is { } reference )
            {
                return Location.Create( reference.SyntaxTree, reference.Span );
            }
        }

        return default;
    }

    private sealed class Analysis
    {
        private readonly ConcurrentDictionary<string, Api> apis = new( StringComparer.Ordinal );
        private readonly ConcurrentBag<Location> incongruent = [];
        private volatile bool unknown;

        public void OnInvocation( SyntaxNodeAnalysisContext context )
        {
            var invocation = (InvocationExpressionSyntax) context.Node;

            if ( context.SemanticModel.GetSymbolInfo( invocation, context.CancellationToken ).Symbol
                 is not IMethodSymbol method )
            {
                return;
            }

            if ( ReplacesNameConvention( context, invocation, method ) )
            {
                return;
            }

            if ( Symbols.ResolveDeclaringType( method )?.ToDisplayString() !=
                 Symbols.EndpointRouteBuilderExtensions ||
                 !Endpoints.IsMapped( method.Name ) )
            {
                return;
            }

            var self = new HashSet<string>( StringComparer.Ordinal );
            var inherited = new HashSet<string>( StringComparer.Ordinal );

            Routes.CollectChainedCalls( invocation, self );
            Routes.ResolveChain( context, Routes.Receiver( invocation ), inherited, out var complete );

            if ( !complete )
            {
                // a group that could not be followed may declare either of the two
                unknown = true;
                return;
            }

            // neutrality declared above the endpoint cannot be narrowed to a version below it, and
            // neither can the two be declared together at the same level
            var neutralAbove = inherited.Contains( IsApiVersionNeutral );
            var versioned = self.Overlaps( VersioningCalls ) || inherited.Overlaps( VersioningCalls );
            var both = self.Contains( IsApiVersionNeutral ) && self.Overlaps( VersioningCalls );

            if ( ( neutralAbove && versioned ) || both )
            {
                incongruent.Add( Symbols.GetLocation( invocation ) );
            }
        }

        /// <remarks>The naming convention decides how controllers collate, and it can be replaced
        /// through the service collection. A replacement that is not one of the built-in trimming
        /// conventions collates by rules that cannot be reproduced here.</remarks>
        private bool ReplacesNameConvention(
            SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax invocation,
            IMethodSymbol method )
        {
            var replaces = false;
            var recognized = false;

            foreach ( var typeArgument in method.TypeArguments )
            {
                var name = typeArgument.ToDisplayString();

                replaces |= name == ControllerNameConventionOf;
                recognized |= TrimmingConventions.Contains( name );
            }

            foreach ( var argument in invocation.ArgumentList.Arguments )
            {
                var type = context.SemanticModel.GetTypeInfo( argument.Expression, context.CancellationToken );

                replaces |= type.Type?.ToDisplayString() == ControllerNameConventionOf ||
                            type.ConvertedType?.ToDisplayString() == ControllerNameConventionOf;

                // the built-in conventions are reached through a property rather than a type
                recognized |= context.SemanticModel.GetSymbolInfo( argument.Expression, context.CancellationToken )
                              .Symbol is IPropertySymbol { IsStatic: true } convention &&
                              convention.ContainingType?.ToDisplayString() == ControllerNameConvention &&
                              TrimmingConventions.Contains( convention.Name );
            }

            if ( replaces && !recognized )
            {
                unknown = true;
            }

            return replaces;
        }

        public void OnNamedType( SymbolAnalysisContext context )
        {
            var type = (INamedTypeSymbol) context.Symbol;

            if ( !Symbols.IsApiController( type ) )
            {
                return;
            }

            if ( !ControllerName.TryResolve( type, out var name ) )
            {
                unknown = true;
                return;
            }

            var api = apis.GetOrAdd( name, static _ => new() );
            var neutral = Symbols.HasAttribute( type, Symbols.ApiVersionNeutralAttribute );

            if ( neutral )
            {
                api.Neutral = true;
            }

            if ( GetLocation( type, Symbols.ApiVersionAttribute ) is { } declared )
            {
                api.Versioned.Add( declared );
            }

            foreach ( var member in type.GetMembers() )
            {
                if ( member is not IMethodSymbol action ||
                     action.MethodKind != MethodKind.Ordinary ||
                     action.DeclaredAccessibility != Accessibility.Public ||
                     action.IsStatic )
                {
                    continue;
                }

                if ( GetLocation( action, Symbols.ApiVersionAttribute ) is not { } version )
                {
                    continue;
                }

                // an action stating both is incongruent on its own, without regard to collation
                if ( Symbols.HasAttribute( action, Symbols.ApiVersionNeutralAttribute ) )
                {
                    incongruent.Add( version );
                }
                else
                {
                    api.Versioned.Add( version );
                }
            }
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            if ( unknown )
            {
                return;
            }

            foreach ( var location in incongruent )
            {
                context.ReportDiagnostic( Diagnostic.Create( AV0019_VersionedAndNeutral, location ) );
            }

            foreach ( var api in apis.Values )
            {
                if ( !api.Neutral )
                {
                    continue;
                }

                foreach ( var location in api.Versioned )
                {
                    context.ReportDiagnostic( Diagnostic.Create( AV0019_VersionedAndNeutral, location ) );
                }
            }
        }

        private sealed class Api
        {
            public ConcurrentBag<Location> Versioned { get; } = [];

            public bool Neutral { get; set; }
        }
    }
}