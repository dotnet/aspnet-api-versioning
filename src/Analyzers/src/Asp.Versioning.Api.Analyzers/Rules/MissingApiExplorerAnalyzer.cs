// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports an OpenAPI document generated without the API explorer that
/// describes the APIs it is generated for.
/// </summary>
/// <remarks>
/// An OpenAPI document is generated from what the API explorer discovered, and what it discovers depends
/// on how the APIs were built. OData and gRPC are each described by an explorer of their own, which
/// nothing else registers on their behalf. Which builder the calls were made against is not tracked,
/// because an application configures API versioning once however the calls are arranged.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class MissingApiExplorerAnalyzer : DiagnosticAnalyzer
{
    private const string AddOpenApi = nameof( AddOpenApi );
    private const string AddOData = nameof( AddOData );
    private const string AddGrpc = nameof( AddGrpc );
    private const string AddApiExplorer = nameof( AddApiExplorer );
    private const string AddODataApiExplorer = nameof( AddODataApiExplorer );
    private const string AddGrpcApiExplorer = nameof( AddGrpcApiExplorer );
    private const string ApiVersioningBuilderExtensions =
        "Microsoft.Extensions.DependencyInjection.IApiVersioningBuilderExtensions";
    private const string VersionedOpenApiOptions = "Asp.Versioning.OpenApi.VersionedOpenApiOptions";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0031_MissingApiExplorer );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        // no document is generated without the library that generates it
        if ( !Symbols.IsReferenced( context.Compilation, VersionedOpenApiOptions ) )
        {
            return;
        }

        var analysis = new Analysis();

        context.RegisterSyntaxNodeAction( analysis.OnInvocation, SyntaxKind.InvocationExpression );
        context.RegisterCompilationEndAction( analysis.OnCompilationEnd );
    }

    private sealed class Analysis
    {
        private readonly ConcurrentBag<Location> openApiCallSites = [];
        private volatile bool odata;
        private volatile bool grpc;
        private volatile bool apiExplorer;
        private volatile bool odataApiExplorer;
        private volatile bool grpcApiExplorer;

        public void OnInvocation( SyntaxNodeAnalysisContext context )
        {
            var invocation = (InvocationExpressionSyntax) context.Node;

            if ( context.SemanticModel.GetSymbolInfo( invocation, context.CancellationToken ).Symbol
                 is not IMethodSymbol method ||
                 Symbols.ResolveDeclaringType( method ) is not { } type ||
                 type.ToDisplayString() != ApiVersioningBuilderExtensions )
            {
                return;
            }

            // the names are shared with the services, which declare unrelated methods of their own
            switch ( method.Name )
            {
                case AddOpenApi:
                    openApiCallSites.Add( Symbols.GetLocation( invocation ) );
                    break;
                case AddOData:
                    odata = true;
                    break;
                case AddGrpc:
                    grpc = true;
                    break;
                case AddApiExplorer:
                    apiExplorer = true;
                    break;
                case AddODataApiExplorer:
                    odataApiExplorer = true;
                    break;
                case AddGrpcApiExplorer:
                    grpcApiExplorer = true;
                    break;
            }
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            if ( openApiCallSites.IsEmpty )
            {
                return;
            }

            var missing = ImmutableArray.CreateBuilder<string>( initialCapacity: 2 );

            if ( odata && !odataApiExplorer )
            {
                missing.Add( AddODataApiExplorer );
            }

            if ( grpc && !grpcApiExplorer )
            {
                missing.Add( AddGrpcApiExplorer );
            }

            // an API built any other way is described by the explorer the rest of them build on, and a
            // specialized explorer can be configured without the APIs it specializes in
            if ( !odata && !grpc && !apiExplorer && !odataApiExplorer && !grpcApiExplorer )
            {
                missing.Add( AddApiExplorer );
            }

            if ( missing.Count == 0 )
            {
                return;
            }

            var explorers = missing.ToImmutable();

            foreach ( var callSite in openApiCallSites )
            {
                for ( var i = 0; i < explorers.Length; i++ )
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create( AV0031_MissingApiExplorer, callSite, explorers[i] ) );
                }
            }
        }
    }
}