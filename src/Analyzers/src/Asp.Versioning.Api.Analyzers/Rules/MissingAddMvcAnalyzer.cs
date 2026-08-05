// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports controller-based APIs which have not been versioned.
/// </summary>
/// <remarks>
/// Whether MVC is versioned cannot be decided from any single statement, so the calls that opt into controllers, into
/// API versioning, and into versioning MVC are collected across the compilation and evaluated once it completes.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class MissingAddMvcAnalyzer : DiagnosticAnalyzer
{
    private const string AddControllers = nameof( AddControllers );
    private const string AddMvcCore = nameof( AddMvcCore );
    private const string AddMvc = nameof( AddMvc );
    private const string AddApiVersioning = nameof( AddApiVersioning );
    private const string MvcServiceCollectionExtensions =
        "Microsoft.Extensions.DependencyInjection.MvcServiceCollectionExtensions";
    private const string MvcCoreServiceCollectionExtensions =
        "Microsoft.Extensions.DependencyInjection.MvcCoreServiceCollectionExtensions";
    private const string ApiVersioningBuilderExtensions =
        "Microsoft.Extensions.DependencyInjection.IApiVersioningBuilderExtensions";
    private const string ServiceCollectionExtensions =
        "Microsoft.Extensions.DependencyInjection.IServiceCollectionExtensions";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0013_MissingAddMvc );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        // controllers cannot exist without MVC, so there is nothing to walk the declared types for
        if ( !Symbols.IsReferenced( context.Compilation, MvcServiceCollectionExtensions ) )
        {
            return;
        }

        var analysis = new Analysis();

        context.RegisterSyntaxNodeAction( analysis.OnInvocation, SyntaxKind.InvocationExpression );
        context.RegisterCompilationEndAction( analysis.OnCompilationEnd );
    }

    // An extension member is declared in a synthetic, nested type that cannot be referred to by name, so the type that
    // declares the member is its containing type
    private static INamedTypeSymbol? ResolveDeclaringType( IMethodSymbol method )
    {
        var type = method.ContainingType;

        return type is { ContainingType: { } declaringType } && !type.CanBeReferencedByName
             ? declaringType
             : type;
    }

    private static Location GetLocation( InvocationExpressionSyntax invocation ) =>
        invocation.Expression is MemberAccessExpressionSyntax access
        ? access.Name.GetLocation()
        : invocation.Expression.GetLocation();

    private sealed class Analysis
    {
        private readonly ConcurrentBag<Location> apiVersioningCallSites = [];
        private volatile bool usesControllers;
        private volatile bool versionsMvc;

        public void OnInvocation( SyntaxNodeAnalysisContext context )
        {
            var invocation = (InvocationExpressionSyntax) context.Node;

            if ( context.SemanticModel.GetSymbolInfo( invocation, context.CancellationToken ).Symbol
                 is not IMethodSymbol method ||
                 ResolveDeclaringType( method ) is not { } type )
            {
                return;
            }

            // the name alone is ambiguous; MVC declares an unrelated AddMvc of its own
            var declaringType = type.ToDisplayString();

            switch ( method.Name )
            {
                case AddControllers when declaringType == MvcServiceCollectionExtensions:
                case AddMvcCore when declaringType == MvcCoreServiceCollectionExtensions:
                    usesControllers = true;
                    break;
                case AddMvc when declaringType == ApiVersioningBuilderExtensions:
                    versionsMvc = true;
                    break;
                case AddApiVersioning when declaringType == ServiceCollectionExtensions:
                    apiVersioningCallSites.Add( GetLocation( invocation ) );
                    break;
            }
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            // without a call to opt into API versioning there is nothing to report against, and nothing to correct
            // because MVC was never versioned in the first place
            if ( !usesControllers || versionsMvc || apiVersioningCallSites.IsEmpty )
            {
                return;
            }

            foreach ( var callSite in apiVersioningCallSites )
            {
                context.ReportDiagnostic( Diagnostic.Create( AV0013_MissingAddMvc, callSite ) );
            }
        }
    }
}