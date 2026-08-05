// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports an option assigned the value it already has.
/// </summary>
/// <remarks>
/// The default of a property is looked up by the type declaring it rather than by name alone, because
/// the same name can carry a different default on a different option. An option whose default is an
/// object rather than a value is left alone, as is the default API version, which is reported on its
/// own because it can be spelled more than one way.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class DefaultValueAnalyzer : DiagnosticAnalyzer
{
    private const string Empty = nameof( Empty );

    private static readonly Dictionary<string, Dictionary<string, object?>> Defaults =
        new( StringComparer.Ordinal )
        {
            [Symbols.ApiVersioningOptions] = new( StringComparer.Ordinal )
            {
                ["RouteConstraintName"] = "apiVersion",
                ["ReportApiVersions"] = false,
                ["AssumeDefaultVersionWhenUnspecified"] = false,
                ["UnsupportedApiVersionStatusCode"] = 400,
            },

            // the values the API explorer shares with API versioning are reported on their own, because
            // what they default to is whatever the versioning options were given rather than what the
            // property was declared with
            [Symbols.ApiExplorerOptions] = new( StringComparer.Ordinal )
            {
                ["GroupNameFormat"] = string.Empty,
                ["SubstitutionFormat"] = "VVV",
                ["SubstituteApiVersionInUrl"] = false,
                ["AddApiVersionParametersWhenVersionNeutral"] = false,
                ["FormatGroupName"] = null,
            },
            [Symbols.ODataApiExplorerOptions] = new( StringComparer.Ordinal )
            {
                ["UseQualifiedNames"] = false,
                ["MetadataOptions"] = 0,
            },
        };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0017_DoNotSetDefaultValue );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        if ( Symbols.IsReferenced( context.Compilation, Symbols.ApiVersioningOptions ) )
        {
            context.RegisterSyntaxNodeAction( OnAssignment, SyntaxKind.SimpleAssignmentExpression );
        }
    }

    private static void OnAssignment( SyntaxNodeAnalysisContext context )
    {
        var assignment = (AssignmentExpressionSyntax) context.Node;

        if ( context.SemanticModel.GetSymbolInfo( assignment.Left, context.CancellationToken ).Symbol
             is not IPropertySymbol property ||
             property.ContainingType?.ToDisplayString() is not { } declaringType ||
             !Defaults.TryGetValue( declaringType, out var defaults ) ||
             !defaults.TryGetValue( property.Name, out var expected ) ||
             !IsDefault( context, assignment.Right, expected ) )
        {
            return;
        }

        context.ReportDiagnostic( Diagnostic.Create( AV0017_DoNotSetDefaultValue, assignment.GetLocation() ) );
    }

    private static bool IsDefault( SyntaxNodeAnalysisContext context, ExpressionSyntax expression, object? expected )
    {
        var constant = context.SemanticModel.GetConstantValue( expression, context.CancellationToken );

        if ( constant.HasValue )
        {
            return Equals( constant.Value, expected );
        }

        // string.Empty is a static field rather than a constant, but it is the same value
        return expected is "" &&
               context.SemanticModel.GetSymbolInfo( expression, context.CancellationToken ).Symbol
               is IFieldSymbol { Name: Empty, ContainingType.SpecialType: SpecialType.System_String };
    }
}