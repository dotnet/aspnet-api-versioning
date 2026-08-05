// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports an API sunset before it is deprecated.
/// </summary>
/// <remarks>
/// Deprecation announces that an API is going away and sunset is when it does, so the two are only in
/// order when deprecation comes first; taking effect on the same day is allowed. Only policies some API
/// reaches together are compared, and only when both state a date that can be read as written. A date
/// that comes from somewhere else is left alone, because what it will be is not decided here.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class PolicyEffectiveDateAnalyzer : DiagnosticAnalyzer
{
    private const string Deprecate = nameof( Deprecate );
    private const string Sunset = nameof( Sunset );
    private const string Effective = nameof( Effective );
    private const string PolicyBuilderExtensions = "Asp.Versioning.IApiVersioningPolicyBuilderExtensions";
    private const string PolicyBuilder = "Asp.Versioning.IApiVersioningPolicyBuilder";
    private const string EffectiveDateExtensions = "Asp.Versioning.IPolicyBuilderExtensions";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0028_SunsetBeforeDeprecation );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        if ( !Symbols.IsReferenced( context.Compilation, PolicyBuilder ) )
        {
            return;
        }

        var analysis = new Analysis();

        context.RegisterSyntaxNodeAction( analysis.OnInvocation, SyntaxKind.InvocationExpression );
        context.RegisterCompilationEndAction( analysis.OnCompilationEnd );
    }

    /// <remarks>The date is stated on the builder the policy returns, which is reached by continuing the
    /// expression that declared it.</remarks>
    private static InvocationExpressionSyntax? FindEffective(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax policy )
    {
        var expression = (ExpressionSyntax) policy;

        while ( expression.Parent is MemberAccessExpressionSyntax access &&
                access.Expression == expression &&
                access.Parent is InvocationExpressionSyntax invocation )
        {
            if ( context.SemanticModel.GetSymbolInfo( invocation, context.CancellationToken ).Symbol
                 is IMethodSymbol { Name: Effective } method &&
                 Symbols.ResolveDeclaringType( method )?.ToDisplayString() == EffectiveDateExtensions )
            {
                return invocation;
            }

            expression = invocation;
        }

        return default;
    }

    /// <remarks>A date is compared as the number it reads as, which orders the same way a date does
    /// without having to be a valid one. A date is stated either as its parts or as a value built from
    /// them, and one that comes from anywhere else cannot be read as written.</remarks>
    private static bool TryResolveDate(
        SyntaxNodeAnalysisContext context,
        SeparatedSyntaxList<ArgumentSyntax> arguments,
        out int date )
    {
        date = 0;

        if ( arguments.Count == 1 )
        {
            return arguments[0].Expression is BaseObjectCreationExpressionSyntax creation &&
                   creation.ArgumentList is { } inner &&
                   TryResolveDate( context, inner.Arguments, out date );
        }

        if ( arguments.Count < 3 )
        {
            return false;
        }

        var parts = new int[3];

        for ( var i = 0; i < parts.Length; i++ )
        {
            var constant = context.SemanticModel.GetConstantValue(
                arguments[i].Expression,
                context.CancellationToken );

            if ( !constant.HasValue || constant.Value is not int part )
            {
                return false;
            }

            parts[i] = part;
        }

        date = ( parts[0] * 10000 ) + ( parts[1] * 100 ) + parts[2];
        return true;
    }

    private sealed class Analysis
    {
        private readonly ConcurrentBag<Policy> deprecations = [];
        private readonly ConcurrentBag<Policy> sunsets = [];

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

            if ( declaringType != PolicyBuilderExtensions && declaringType != PolicyBuilder )
            {
                return;
            }

            var declared = method.Name switch
            {
                Deprecate => deprecations,
                Sunset => sunsets,
                _ => default,
            };

            if ( declared is null ||
                 !PolicyKey.TryResolve( context, invocation, method, out var key ) ||
                 key.Unreachable ||
                 FindEffective( context, invocation ) is not { } effective ||
                 !TryResolveDate( context, effective.ArgumentList.Arguments, out var date ) )
            {
                return;
            }

            declared.Add( new( key, date, Symbols.GetLocation( effective ) ) );
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            if ( deprecations.IsEmpty || sunsets.IsEmpty )
            {
                return;
            }

            foreach ( var sunset in sunsets )
            {
                foreach ( var deprecation in deprecations )
                {
                    // taking effect on the same day is in order, and only an earlier one is not
                    if ( sunset.Date < deprecation.Date && sunset.Key.Intersects( deprecation.Key ) )
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create( AV0028_SunsetBeforeDeprecation, sunset.Location ) );
                        break;
                    }
                }
            }
        }

        private sealed class Policy( PolicyKey key, int date, Location location )
        {
            public PolicyKey Key { get; } = key;

            public int Date { get; } = date;

            public Location Location { get; } = location;
        }
    }
}