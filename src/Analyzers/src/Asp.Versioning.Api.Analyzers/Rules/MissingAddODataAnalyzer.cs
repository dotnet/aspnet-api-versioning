// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports OData APIs which have not been versioned.
/// </summary>
/// <remarks>
/// OData routes by its own conventions rather than by the routes API versioning otherwise observes, so
/// versioning an OData API takes an explicit opt in. The API Explorer variant registers the versioned
/// services it needs on its own, which is a supported way to describe an OData API without taking on the
/// rest of them.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class MissingAddODataAnalyzer : DiagnosticAnalyzer
{
    private const string AddOData = nameof( AddOData );
    private const string AddODataApiExplorer = nameof( AddODataApiExplorer );
    private const string AddApiVersioning = nameof( AddApiVersioning );
    private const string ODataMvcBuilderExtensions =
        "Microsoft.AspNetCore.OData.ODataMvcBuilderExtensions";
    private const string ODataMvcCoreBuilderExtensions =
        "Microsoft.AspNetCore.OData.ODataMvcCoreBuilderExtensions";
    private const string ApiVersioningBuilderExtensions =
        "Microsoft.Extensions.DependencyInjection.IApiVersioningBuilderExtensions";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0022_MissingAddOData );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        // OData cannot be registered without the library that declares it, so there is nothing to match
        if ( !Symbols.IsReferenced( context.Compilation, ODataMvcBuilderExtensions ) )
        {
            return;
        }

        var analysis = new Analysis();

        context.RegisterSyntaxNodeAction( analysis.OnInvocation, SyntaxKind.InvocationExpression );
        context.RegisterCompilationEndAction( analysis.OnCompilationEnd );
    }

    private sealed class Analysis
    {
        private readonly ConcurrentBag<Location> apiVersioningCallSites = [];
        private volatile bool usesOData;
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

            // the name alone is ambiguous; OData declares an AddOData of its own for MVC
            var declaringType = type.ToDisplayString();

            switch ( method.Name )
            {
                case AddOData when declaringType == ODataMvcBuilderExtensions:
                case AddOData when declaringType == ODataMvcCoreBuilderExtensions:
                    usesOData = true;
                    break;
                case AddOData when declaringType == ApiVersioningBuilderExtensions:
                case AddODataApiExplorer when declaringType == ApiVersioningBuilderExtensions:
                    versionsOData = true;
                    break;
                case AddApiVersioning when declaringType == Symbols.ServiceCollectionExtensions:
                    apiVersioningCallSites.Add( Symbols.GetLocation( invocation ) );
                    break;
            }
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            // without a call to opt into API versioning there is nothing to report against, and nothing
            // to correct because OData was never versioned in the first place
            if ( !usesOData || versionsOData || apiVersioningCallSites.IsEmpty )
            {
                return;
            }

            foreach ( var callSite in apiVersioningCallSites )
            {
                context.ReportDiagnostic( Diagnostic.Create( AV0022_MissingAddOData, callSite ) );
            }
        }
    }
}