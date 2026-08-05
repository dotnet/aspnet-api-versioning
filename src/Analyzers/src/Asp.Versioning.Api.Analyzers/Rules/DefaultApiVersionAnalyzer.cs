// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports a default API version which is either already the default or cannot be a default
/// at all.
/// </summary>
/// <remarks>The options are matched by name because the API surface that declares them is not available to an analyzer,
/// which the compiler requires to target netstandard2.0. A version-neutral default is invalid wherever it is written,
/// whereas a redundant one is only redundant against the options that decide the default in the first place.</remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class DefaultApiVersionAnalyzer : DiagnosticAnalyzer
{
    private const string DefaultApiVersion = nameof( DefaultApiVersion );
    private const string Default = nameof( Default );
    private const string Neutral = nameof( Neutral );
    private const string ApiVersion = "Asp.Versioning.ApiVersion";

    private static readonly HashSet<string> OptionsTypes = new( StringComparer.Ordinal )
    {
        "Asp.Versioning.ApiVersioningOptions",
        "Asp.Versioning.ApiExplorer.ApiExplorerOptions",
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            AV0011_UnnecessaryDefaultApiVersion,
            AV0012_NeutralDefaultApiVersion );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();

        // an object initializer assigns through the same expression as a property does
        context.RegisterSyntaxNodeAction( OnAssignment, SyntaxKind.SimpleAssignmentExpression );
    }

    private static void OnAssignment( SyntaxNodeAnalysisContext context )
    {
        var assignment = (AssignmentExpressionSyntax) context.Node;
        var assigned = context.SemanticModel.GetSymbolInfo( assignment.Left, context.CancellationToken ).Symbol;

        if ( assigned is not IPropertySymbol { Name: DefaultApiVersion } property ||
             !IsVersioningOptions( property.ContainingType ) )
        {
            return;
        }

        if ( Classify( context, assignment.Right ) is not { } descriptor )
        {
            return;
        }

        // the API explorer is given whatever default the versioning options were given, so a version that
        // matches is reported against what it came from rather than against the version declared here
        if ( ReferenceEquals( descriptor, AV0011_UnnecessaryDefaultApiVersion ) &&
             !Symbols.Declares( property.ContainingType, Symbols.ApiVersioningOptions ) )
        {
            return;
        }

        // the whole assignment is what is unnecessary and gets faded out, whereas a neutral version is a problem with
        // the value alone
        var location = ReferenceEquals( descriptor, AV0011_UnnecessaryDefaultApiVersion )
                     ? assignment.GetLocation()
                     : assignment.Right.GetLocation();

        context.ReportDiagnostic( Diagnostic.Create( descriptor, location ) );
    }

    private static DiagnosticDescriptor? Classify( SyntaxNodeAnalysisContext context, ExpressionSyntax expression )
    {
        var symbol = context.SemanticModel.GetSymbolInfo( expression, context.CancellationToken ).Symbol;

        if ( symbol is IPropertySymbol { IsStatic: true } wellKnown && IsApiVersion( wellKnown.ContainingType ) )
        {
            return wellKnown.Name switch
            {
                Default => AV0011_UnnecessaryDefaultApiVersion,
                Neutral => AV0012_NeutralDefaultApiVersion,
                _ => default,
            };
        }

        if ( expression is BaseObjectCreationExpressionSyntax creation &&
             symbol is IMethodSymbol ctor &&
             IsApiVersion( ctor.ContainingType ) &&
             IsDefaultApiVersion( context, ctor, creation.ArgumentList ) )
        {
            return AV0011_UnnecessaryDefaultApiVersion;
        }

        return default;
    }

    private static bool IsDefaultApiVersion(
        SyntaxNodeAnalysisContext context,
        IMethodSymbol ctor,
        ArgumentListSyntax? list )
    {
        var arguments = list is null ? default : list.Arguments;
        var version = default( double? );
        var major = default( int? );
        var minor = default( int? );

        for ( var i = 0; i < arguments.Count; i++ )
        {
            var argument = arguments[i];
            var parameter = ResolveParameter( ctor.Parameters, argument.NameColon?.Name.Identifier.ValueText, i );

            if ( parameter is null )
            {
                return false;
            }

            var constant = context.SemanticModel.GetConstantValue( argument.Expression, context.CancellationToken );

            if ( !constant.HasValue )
            {
                return false;
            }

            switch ( parameter.Name )
            {
                case "version" when constant.Value is double number:
                    version = number;
                    break;
                case "version" when constant.Value is int number:
                    version = number;
                    break;
                case "majorVersion" when constant.Value is int number:
                    major = number;
                    break;
                case "minorVersion" when constant.Value is null:
                    break;
                case "minorVersion" when constant.Value is int number:
                    minor = number;
                    break;
                case "status" when constant.Value is null:
                    break;
                default:
                    return false;
            }
        }

        return version is { } value ? value == 1d : major == 1 && minor is null or 0;
    }

    private static IParameterSymbol? ResolveParameter(
        ImmutableArray<IParameterSymbol> parameters,
        string? name,
        int index )
    {
        if ( name is null )
        {
            return index < parameters.Length ? parameters[index] : default;
        }

        foreach ( var parameter in parameters )
        {
            if ( parameter.Name == name )
            {
                return parameter;
            }
        }

        return default;
    }

    private static bool IsVersioningOptions( INamedTypeSymbol? type )
    {
        for ( var declaringType = type; declaringType is not null; declaringType = declaringType.BaseType )
        {
            if ( OptionsTypes.Contains( declaringType.ToDisplayString() ) )
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsApiVersion( INamedTypeSymbol? type ) => type?.ToDisplayString() == ApiVersion;
}