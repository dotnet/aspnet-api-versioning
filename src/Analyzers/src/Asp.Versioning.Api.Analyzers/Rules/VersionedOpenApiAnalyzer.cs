// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports OpenAPI configured without regard to API versions.
/// </summary>
/// <remarks>
/// Versioned OpenAPI registers services of its own in place of the ones OpenAPI registers for itself,
/// which describe a single document that knows nothing about API versions. The endpoint serving the
/// documents resolves them from the services of the request it is answering, which is only where the
/// versioned documents are to be found once the endpoint has been told to look there.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class VersionedOpenApiAnalyzer : DiagnosticAnalyzer
{
    private const string AddApiExplorer = nameof( AddApiExplorer );
    private const string AddODataApiExplorer = nameof( AddODataApiExplorer );
    private const string AddGrpcApiExplorer = nameof( AddGrpcApiExplorer );
    private const string AddOpenApi = nameof( AddOpenApi );
    private const string MapOpenApi = nameof( MapOpenApi );
    private const string WithDocumentPerVersion = nameof( WithDocumentPerVersion );
    private const string ApiVersioningBuilderExtensions =
        "Microsoft.Extensions.DependencyInjection.IApiVersioningBuilderExtensions";
    private const string OpenApiServiceCollectionExtensions =
        "Microsoft.Extensions.DependencyInjection.OpenApiServiceCollectionExtensions";
    private const string OpenApiEndpointRouteBuilderExtensions =
        "Microsoft.AspNetCore.Builder.OpenApiEndpointRouteBuilderExtensions";
    private const string EndpointConventionBuilderExtensions =
        "Microsoft.AspNetCore.Builder.IEndpointConventionBuilderExtensions";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            AV0029_UnnecessaryOpenApiServices,
            AV0030_MissingDocumentPerVersion );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        // neither the services nor the endpoint exist to be configured without the library declaring them
        if ( !Symbols.IsReferenced( context.Compilation, OpenApiServiceCollectionExtensions ) )
        {
            return;
        }

        var analysis = new Analysis();

        context.RegisterSyntaxNodeAction( analysis.OnInvocation, SyntaxKind.InvocationExpression );
        context.RegisterCompilationEndAction( analysis.OnCompilationEnd );
    }

    private static bool IsCall( SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, string name, string declaringType ) =>
        context.SemanticModel.GetSymbolInfo( invocation, context.CancellationToken ).Symbol
        is IMethodSymbol method &&
        method.Name == name &&
        Symbols.ResolveDeclaringType( method )?.ToDisplayString() == declaringType;

    /// <remarks>The endpoint is told to serve a document per version by continuing the expression that
    /// mapped it.</remarks>
    private static bool IsDocumentPerVersion( SyntaxNodeAnalysisContext context, InvocationExpressionSyntax mapped )
    {
        var expression = (ExpressionSyntax) mapped;

        while ( expression.Parent is MemberAccessExpressionSyntax access &&
                access.Expression == expression &&
                access.Parent is InvocationExpressionSyntax invocation )
        {
            if ( IsCall( context, invocation, WithDocumentPerVersion, EndpointConventionBuilderExtensions ) )
            {
                return true;
            }

            expression = invocation;
        }

        return false;
    }

    /// <remarks>Reading the expression the other way tells whether a convention belongs to an endpoint
    /// this can see, because one applied anywhere else is applied to something unknown.</remarks>
    private static bool FollowsMapOpenApi( SyntaxNodeAnalysisContext context, InvocationExpressionSyntax decoration )
    {
        var expression = decoration.Expression;

        while ( expression is MemberAccessExpressionSyntax access )
        {
            if ( access.Expression is not InvocationExpressionSyntax inner )
            {
                return false;
            }

            if ( IsCall( context, inner, MapOpenApi, OpenApiEndpointRouteBuilderExtensions ) )
            {
                return true;
            }

            expression = inner.Expression;
        }

        return false;
    }

    private sealed class Analysis
    {
        private readonly ConcurrentBag<Location> serviceCallSites = [];
        private readonly ConcurrentBag<Location> mappedCallSites = [];
        private volatile bool versioned;
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

            // OpenAPI declares an AddOpenApi of its own, which is the one that knows nothing of versions
            var declaringType = type.ToDisplayString();

            switch ( method.Name )
            {
                case AddApiExplorer or AddODataApiExplorer or AddGrpcApiExplorer or AddOpenApi
                    when declaringType == ApiVersioningBuilderExtensions:
                    versioned = true;
                    break;
                case AddOpenApi when declaringType == OpenApiServiceCollectionExtensions:
                    serviceCallSites.Add(
                        invocation.Parent is ExpressionStatementSyntax statement
                        ? statement.GetLocation()
                        : invocation.GetLocation() );
                    break;
                case MapOpenApi when declaringType == OpenApiEndpointRouteBuilderExtensions:
                    if ( !IsDocumentPerVersion( context, invocation ) )
                    {
                        mappedCallSites.Add( Symbols.GetLocation( invocation ) );
                    }

                    break;
                case WithDocumentPerVersion when declaringType == EndpointConventionBuilderExtensions:
                    // applied to an endpoint reached some other way, which may well be the mapped one
                    if ( !FollowsMapOpenApi( context, invocation ) )
                    {
                        unknown = true;
                    }

                    break;
            }
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            if ( !versioned )
            {
                return;
            }

            foreach ( var callSite in serviceCallSites )
            {
                context.ReportDiagnostic( Diagnostic.Create( AV0029_UnnecessaryOpenApiServices, callSite ) );
            }

            if ( unknown )
            {
                return;
            }

            foreach ( var callSite in mappedCallSites )
            {
                context.ReportDiagnostic( Diagnostic.Create( AV0030_MissingDocumentPerVersion, callSite ) );
            }
        }
    }
}