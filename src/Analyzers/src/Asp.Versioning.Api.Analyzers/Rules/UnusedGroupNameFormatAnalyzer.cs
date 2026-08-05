// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports a group name format which nothing is formatted by.
/// </summary>
/// <remarks>
/// The callback is only reached for an API that has a group name; an API without one is described by its
/// API version alone. A group name can be stated by a controller, by a mapped endpoint, or by a group of
/// them, and one anywhere in the application is enough to put the callback to use. Group names can also be
/// supplied by an implementation of their own, which says nothing about whether any are set.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class UnusedGroupNameFormatAnalyzer : DiagnosticAnalyzer
{
    private const string FormatGroupName = nameof( FormatGroupName );
    private const string GroupName = nameof( GroupName );
    private const string WithGroupName = nameof( WithGroupName );
    private const string ApiExplorerSettingsAttribute = "Microsoft.AspNetCore.Mvc.ApiExplorerSettingsAttribute";
    private const string EndpointGroupNameAttribute = "Microsoft.AspNetCore.Routing.EndpointGroupNameAttribute";
    private const string RoutingEndpointConventionBuilderExtensions =
        "Microsoft.AspNetCore.Builder.RoutingEndpointConventionBuilderExtensions";
    private const string ApiDescriptionGroupNameProvider =
        "Microsoft.AspNetCore.Mvc.ApiExplorer.IApiDescriptionGroupNameProvider";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0026_UnusedGroupNameFormat );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        if ( !Symbols.IsReferenced( context.Compilation, Symbols.ApiExplorerOptions ) )
        {
            return;
        }

        var analysis = new Analysis();

        // an object initializer assigns through the same expression as a property does
        context.RegisterSyntaxNodeAction( analysis.OnAssignment, SyntaxKind.SimpleAssignmentExpression );
        context.RegisterSyntaxNodeAction( analysis.OnAttribute, SyntaxKind.Attribute );
        context.RegisterSyntaxNodeAction( analysis.OnInvocation, SyntaxKind.InvocationExpression );
        context.RegisterSymbolAction( analysis.OnNamedType, SymbolKind.NamedType );
        context.RegisterCompilationEndAction( analysis.OnCompilationEnd );
    }

    /// <remarks>A name that cannot be read as it is written may still be one at run time, so it counts as
    /// a name rather than as the absence of one.</remarks>
    private static bool IsGroupName( SyntaxNodeAnalysisContext context, ExpressionSyntax expression )
    {
        var constant = context.SemanticModel.GetConstantValue( expression, context.CancellationToken );

        return !constant.HasValue ||
               ( constant.Value is string name && !string.IsNullOrEmpty( name ) );
    }

    private sealed class Analysis
    {
        private readonly ConcurrentBag<Location> formatCallSites = [];
        private volatile bool grouped;
        private volatile bool surfaced;
        private volatile bool unknown;

        public void OnAssignment( SyntaxNodeAnalysisContext context )
        {
            var assignment = (AssignmentExpressionSyntax) context.Node;

            // a callback that is cleared rather than provided is never reached to begin with
            if ( context.SemanticModel.GetSymbolInfo( assignment.Left, context.CancellationToken ).Symbol
                 is not IPropertySymbol { Name: FormatGroupName } property ||
                 !Symbols.Declares( property.ContainingType, Symbols.ApiExplorerOptions ) ||
                 assignment.Right.IsKind( SyntaxKind.NullLiteralExpression ) ||
                 assignment.Right.IsKind( SyntaxKind.DefaultLiteralExpression ) )
            {
                return;
            }

            formatCallSites.Add( assignment.GetLocation() );
        }

        public void OnAttribute( SyntaxNodeAnalysisContext context )
        {
            var attribute = (AttributeSyntax) context.Node;

            if ( attribute.ArgumentList is not { } list ||
                 context.SemanticModel.GetSymbolInfo( attribute, context.CancellationToken ).Symbol
                 is not IMethodSymbol constructor )
            {
                return;
            }

            var type = constructor.ContainingType;

            if ( Symbols.Declares( type, ApiExplorerSettingsAttribute ) )
            {
                foreach ( var argument in list.Arguments )
                {
                    if ( argument.NameEquals?.Name.Identifier.ValueText == GroupName &&
                         IsGroupName( context, argument.Expression ) )
                    {
                        grouped = true;
                        return;
                    }
                }
            }
            else if ( Symbols.Declares( type, EndpointGroupNameAttribute ) &&
                      list.Arguments.Count > 0 &&
                      IsGroupName( context, list.Arguments[0].Expression ) )
            {
                grouped = true;
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
            var arguments = invocation.ArgumentList.Arguments;

            if ( method.Name == WithGroupName && declaringType == RoutingEndpointConventionBuilderExtensions )
            {
                if ( arguments.Count > 0 && IsGroupName( context, arguments[0].Expression ) )
                {
                    grouped = true;
                }
            }
            else if ( declaringType == Symbols.EndpointRouteBuilderExtensions && Endpoints.IsMapped( method.Name ) )
            {
                surfaced = true;
            }
        }

        public void OnNamedType( SymbolAnalysisContext context )
        {
            var type = (INamedTypeSymbol) context.Symbol;

            if ( Symbols.IsApiController( type ) )
            {
                surfaced = true;
            }

            foreach ( var contract in type.AllInterfaces )
            {
                if ( contract.ToDisplayString() == ApiDescriptionGroupNameProvider )
                {
                    unknown = true;
                    break;
                }
            }
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            // an application whose APIs are declared elsewhere keeps its group names there as well
            if ( grouped || unknown || !surfaced || formatCallSites.IsEmpty )
            {
                return;
            }

            foreach ( var callSite in formatCallSites )
            {
                context.ReportDiagnostic( Diagnostic.Create( AV0026_UnusedGroupNameFormat, callSite ) );
            }
        }
    }
}