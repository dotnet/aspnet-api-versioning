// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that validates the arguments projected onto an API version.
/// </summary>
/// <remarks>
/// The numeric and date components of an API version carry no string syntax to key off of, so the API surface that
/// accepts them is matched by name and each argument is then validated according to the parameter it is bound to.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class ApiVersionArgumentsMustBeValid : DiagnosticAnalyzer
{
    private static readonly HashSet<string> DeclaringTypes = new( StringComparer.Ordinal )
    {
        "Asp.Versioning.ApiVersionAttribute",
        "Asp.Versioning.AdvertiseApiVersionsAttribute",
        "Asp.Versioning.MapToApiVersionAttribute",
        "Asp.Versioning.Conventions.ApiVersionConventionBuilderExtensions",
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            AV0003_InvalidApiVersionStatus,
            AV0004_InvalidApiVersionNumber,
            AV0005_InvalidApiVersionYear,
            AV0006_InvalidApiVersionMonth,
            AV0007_InvalidApiVersionDay,
            AV0008_InvalidApiVersionDate );

    public override void Initialize( AnalysisContext context )
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

    private static void OnAttribute( SyntaxNodeAnalysisContext context )
    {
        var attribute = (AttributeSyntax) context.Node;

        if ( attribute.ArgumentList is not { Arguments.Count: > 0 } list ||
             !TryGetDeclaredApi( context, attribute, out var ctor ) )
        {
            return;
        }

        var arguments = list.Arguments;
        var date = default( DateArguments );

        for ( var i = 0; i < arguments.Count; i++ )
        {
            var argument = arguments[i];

            // a named argument is an initializer for a property or field, which is never a version component
            if ( argument.NameEquals is not null )
            {
                continue;
            }

            var parameter = Arguments.ResolveParameter( ctor.Parameters, argument.NameColon?.Name.Identifier.ValueText, i );

            Validate( context, parameter, argument.Expression, ref date );
        }

        ValidateDate( context, ref date );
    }

    private static void OnInvocation( SyntaxNodeAnalysisContext context )
    {
        var invocation = (InvocationExpressionSyntax) context.Node;

        ValidateArguments( context, invocation, invocation.ArgumentList );
    }

    private static void OnObjectCreation( SyntaxNodeAnalysisContext context )
    {
        var creation = (BaseObjectCreationExpressionSyntax) context.Node;

        ValidateArguments( context, creation, creation.ArgumentList );
    }

    private static void ValidateArguments( SyntaxNodeAnalysisContext context, SyntaxNode node, ArgumentListSyntax? list )
    {
        if ( list is not { Arguments.Count: > 0 } || !TryGetDeclaredApi( context, node, out var method ) )
        {
            return;
        }

        var arguments = list.Arguments;
        var date = default( DateArguments );

        for ( var i = 0; i < arguments.Count; i++ )
        {
            var argument = arguments[i];
            var parameter = Arguments.ResolveParameter( method.Parameters, argument.NameColon?.Name.Identifier.ValueText, i );

            Validate( context, parameter, argument.Expression, ref date );
        }

        ValidateDate( context, ref date );
    }

    private static bool TryGetDeclaredApi( SyntaxNodeAnalysisContext context, SyntaxNode node, out IMethodSymbol method )
    {
        if ( context.SemanticModel.GetSymbolInfo( node, context.CancellationToken ).Symbol is IMethodSymbol symbol &&
             Arguments.ResolveDeclaringType( symbol ) is { } type &&
             DeclaringTypes.Contains( type.ToDisplayString() ) )
        {
            method = symbol;
            return true;
        }

        method = default!;
        return false;
    }

    private static void Validate(
        SyntaxNodeAnalysisContext context,
        IParameterSymbol? parameter,
        ExpressionSyntax expression,
        ref DateArguments date )
    {
        if ( parameter is null )
        {
            return;
        }

        // a name alone is ambiguous; a version can also be a string and a date can also be a group version
        switch ( parameter.Name )
        {
            case "version" when parameter.Type.SpecialType == SpecialType.System_Double:
            case "majorVersion" or "minorVersion" when IsInt32( parameter.Type ):
                ValidateNumber( context, expression );
                break;
            case "otherVersions" when parameter.Type is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Double }:
                ValidateNumbers( context, expression );
                break;
            case "year" when IsInt32( parameter.Type ):
                date.Year = Capture( context, expression, AV0005_InvalidApiVersionYear, ApiVersionValidator.IsValidYear );
                break;
            case "month" when IsInt32( parameter.Type ):
                date.Month = Capture( context, expression, AV0006_InvalidApiVersionMonth, ApiVersionValidator.IsValidMonth );
                break;
            case "day" when IsInt32( parameter.Type ):
                date.Day = Capture( context, expression, AV0007_InvalidApiVersionDay, ApiVersionValidator.IsValidDay );
                break;
            case "status" when parameter.Type.SpecialType == SpecialType.System_String:
                ValidateStatus( context, expression );
                break;
        }
    }

    private static void ValidateNumbers( SyntaxNodeAnalysisContext context, ExpressionSyntax expression )
    {
        // a params array can be passed as an array rather than expanded; validate each element
        if ( Arguments.GetArrayElements( expression ) is { } elements )
        {
            foreach ( var element in elements )
            {
                ValidateNumber( context, element );
            }

            return;
        }

        ValidateNumber( context, expression );
    }

    private static void ValidateNumber( SyntaxNodeAnalysisContext context, ExpressionSyntax expression )
    {
        var constant = context.SemanticModel.GetConstantValue( expression, context.CancellationToken );

        // only a compile-time constant can be validated; anything else is unknowable until run time
        var valid = constant switch
        {
            { HasValue: true, Value: double number } => ApiVersionValidator.IsValidNumber( number ),
            { HasValue: true, Value: int number } => ApiVersionValidator.IsValidNumber( number ),
            _ => true,
        };

        if ( !valid )
        {
            context.ReportDiagnostic( Diagnostic.Create( AV0004_InvalidApiVersionNumber, expression.GetLocation() ) );
        }
    }

    private static void ValidateStatus( SyntaxNodeAnalysisContext context, ExpressionSyntax expression )
    {
        var constant = context.SemanticModel.GetConstantValue( expression, context.CancellationToken );

        if ( constant is { HasValue: true, Value: string status } && !ApiVersionValidator.IsValidStatus( status ) )
        {
            context.ReportDiagnostic( Diagnostic.Create( AV0003_InvalidApiVersionStatus, expression.GetLocation() ) );
        }
    }

    private static DateArgument Capture(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        DiagnosticDescriptor descriptor,
        Func<int, bool> isValid )
    {
        var constant = context.SemanticModel.GetConstantValue( expression, context.CancellationToken );

        if ( constant is not { HasValue: true, Value: int component } )
        {
            return default;
        }

        var valid = isValid( component );

        if ( !valid )
        {
            context.ReportDiagnostic( Diagnostic.Create( descriptor, expression.GetLocation() ) );
        }

        return new() { Expression = expression, Value = component, Valid = valid };
    }

    private static void ValidateDate( SyntaxNodeAnalysisContext context, ref DateArguments date )
    {
        var year = date.Year;
        var month = date.Month;
        var day = date.Day;

        // the composed date is only meaningful once every component is known and individually in range
        if ( !year.Valid || !month.Valid || !day.Valid )
        {
            return;
        }

        if ( ApiVersionValidator.IsValidDate( year.Value, month.Value, day.Value ) )
        {
            return;
        }

        var span = TextSpan.FromBounds( year.Expression!.SpanStart, day.Expression!.Span.End );
        var location = Location.Create( year.Expression.SyntaxTree, span );

        context.ReportDiagnostic( Diagnostic.Create( AV0008_InvalidApiVersionDate, location ) );
    }

    private static bool IsInt32( ITypeSymbol type ) =>
        type.SpecialType == SpecialType.System_Int32 ||
        ( type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable &&
          nullable.TypeArguments[0].SpecialType == SpecialType.System_Int32 );

    private struct DateArgument
    {
        public ExpressionSyntax? Expression;
        public int Value;
        public bool Valid;
    }

    private struct DateArguments
    {
        public DateArgument Year;
        public DateArgument Month;
        public DateArgument Day;
    }
}