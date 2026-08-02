// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

using System.Collections.Immutable;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents the base implementation of an analyzer that validates values annotated with a
/// <see cref="StringSyntaxAttribute">string syntax</see>.
/// </summary>
/// <remarks>
/// Reports a diagnostic for every compile-time constant passed to a parameter, property, or field
/// annotated with <c>[StringSyntax]</c> for a particular syntax, whose value fails validation. The
/// annotation is discovered on the resolved symbol rather than on a known set of types, so any API
/// annotated with the syntax is covered without being enumerated here.
/// </remarks>
public abstract class StringSyntaxAnalyzer : DiagnosticAnalyzer
{
    private const string StringSyntaxAttribute = nameof( StringSyntaxAttribute );
    private const string StringSyntaxNamespace = "System.Diagnostics.CodeAnalysis";
    private readonly string syntax;

    protected StringSyntaxAnalyzer( string syntax, params DiagnosticDescriptor[] descriptors )
    {
        this.syntax = syntax;
        SupportedDiagnostics = ImmutableArray.Create( descriptors );
    }

    public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

    public sealed override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction( OnAttribute, SyntaxKind.Attribute );
        context.RegisterSyntaxNodeAction( OnInvocation, SyntaxKind.InvocationExpression );
        context.RegisterSyntaxNodeAction(
            OnObjectCreation,
            SyntaxKind.ObjectCreationExpression,
            SyntaxKind.ImplicitObjectCreationExpression );
    }

    protected abstract void Validate( string text, Reporter reporter );

    /// <summary>
    /// Reports the diagnostics found in an annotated value.
    /// </summary>
    /// <remarks>The value is reported at the location of the expression that produced it. A constant
    /// may be declared elsewhere, and the offsets within a literal do not survive escaping, so a
    /// diagnostic identifies the offending part of the value through its message.</remarks>
    protected readonly struct Reporter
    {
        private readonly SyntaxNodeAnalysisContext context;
        private readonly Location location;

        internal Reporter( SyntaxNodeAnalysisContext context, Location location )
        {
            this.context = context;
            this.location = location;
        }

        public void Report( DiagnosticDescriptor descriptor, params object?[] messageArgs ) =>
            context.ReportDiagnostic( Diagnostic.Create( descriptor, location, messageArgs ) );
    }

    private void OnAttribute( SyntaxNodeAnalysisContext context )
    {
        var attribute = (AttributeSyntax) context.Node;

        if ( attribute.ArgumentList is not { Arguments.Count: > 0 } list ||
             context.SemanticModel.GetSymbolInfo( attribute, context.CancellationToken ).Symbol is not IMethodSymbol ctor )
        {
            return;
        }

        var arguments = list.Arguments;

        for ( var i = 0; i < arguments.Count; i++ )
        {
            var argument = arguments[i];

            // a named argument is an initializer for a property or field; everything else maps to a parameter
            var target = argument.NameEquals is { } nameEquals
                ? Arguments.ResolveMember( ctor.ContainingType, nameEquals.Name.Identifier.ValueText )
                : Arguments.ResolveParameter( ctor.Parameters, argument.NameColon?.Name.Identifier.ValueText, i );

            if ( HasStringSyntax( target ) )
            {
                Validate( context, argument.Expression );
            }
        }
    }

    private void OnInvocation( SyntaxNodeAnalysisContext context )
    {
        var invocation = (InvocationExpressionSyntax) context.Node;
        var symbol = context.SemanticModel.GetSymbolInfo( invocation, context.CancellationToken ).Symbol;

        ValidateArguments( context, invocation.ArgumentList, symbol );
    }

    private void OnObjectCreation( SyntaxNodeAnalysisContext context )
    {
        var creation = (BaseObjectCreationExpressionSyntax) context.Node;
        var symbol = context.SemanticModel.GetSymbolInfo( creation, context.CancellationToken ).Symbol;

        ValidateArguments( context, creation.ArgumentList, symbol );
    }

    private void ValidateArguments( SyntaxNodeAnalysisContext context, ArgumentListSyntax? list, ISymbol? symbol )
    {
        if ( list is not { Arguments.Count: > 0 } || symbol is not IMethodSymbol method )
        {
            return;
        }

        var arguments = list.Arguments;

        for ( var i = 0; i < arguments.Count; i++ )
        {
            var argument = arguments[i];
            var parameter = Arguments.ResolveParameter( method.Parameters, argument.NameColon?.Name.Identifier.ValueText, i );

            if ( HasStringSyntax( parameter ) )
            {
                Validate( context, argument.Expression );
            }
        }
    }

    private void Validate( SyntaxNodeAnalysisContext context, ExpressionSyntax expression )
    {
        // a params array can be passed as an array rather than expanded; validate each element
        if ( Arguments.GetArrayElements( expression ) is { } elements )
        {
            foreach ( var element in elements )
            {
                Validate( context, element );
            }

            return;
        }

        var constant = context.SemanticModel.GetConstantValue( expression, context.CancellationToken );

        // only a compile-time constant can be validated; anything else is unknowable until run time
        if ( !constant.HasValue || constant.Value is not string text )
        {
            return;
        }

        Validate( text, new Reporter( context, expression.GetLocation() ) );
    }

    private bool HasStringSyntax( ISymbol? symbol )
    {
        if ( symbol is null )
        {
            return false;
        }

        foreach ( var attribute in symbol.GetAttributes() )
        {
            // the attribute is internal in some assemblies and defined by the BCL in others,
            // so it is matched by name rather than a resolved type symbol
            if ( attribute.AttributeClass is { Name: StringSyntaxAttribute } type &&
                 type.ContainingNamespace?.ToDisplayString() == StringSyntaxNamespace &&
                 attribute.ConstructorArguments.Length > 0 &&
                 attribute.ConstructorArguments[0].Value as string == syntax )
            {
                return true;
            }
        }

        return false;
    }
}