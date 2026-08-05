// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using static Descriptor;
using static Microsoft.CodeAnalysis.Diagnostics.GeneratedCodeAnalysisFlags;

/// <summary>
/// Represents an analyzer that reports an API explorer option restating the value it already has.
/// </summary>
/// <remarks>
/// The API explorer takes the options it shares with API versioning before its own configuration runs, so
/// stating a shared value again only repeats what it was already given. A value that differs is a
/// deliberate departure from the versioning options and is left alone.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class InheritedApiExplorerOptionAnalyzer : DiagnosticAnalyzer
{
    /// <remarks>The two options do not always agree on the name of a value they share.</remarks>
    private static readonly Dictionary<string, string> Shared = new( StringComparer.Ordinal )
    {
        ["AssumeDefaultVersionWhenUnspecified"] = "AssumeDefaultVersionWhenUnspecified",
        ["ApiVersionParameterSource"] = "ApiVersionReader",
        ["DefaultApiVersion"] = "DefaultApiVersion",
        ["RouteConstraintName"] = "RouteConstraintName",
        ["ApiVersionSelector"] = "ApiVersionSelector",
    };

    /// <remarks>The value the API explorer is given when the versioning options state none of their own.
    /// The default selector is built from the options it belongs to and cannot be written by hand, so
    /// there is nothing for a selector to match.</remarks>
    private static readonly Dictionary<string, string?> Defaults = new( StringComparer.Ordinal )
    {
        ["AssumeDefaultVersionWhenUnspecified"] = OptionValue.Constant( false ),
        ["ApiVersionReader"] = OptionValue.Member( "Asp.Versioning.ApiVersionReader.Default" ),
        ["DefaultApiVersion"] = OptionValue.DefaultApiVersion,
        ["RouteConstraintName"] = OptionValue.Constant( "apiVersion" ),
        ["ApiVersionSelector"] = default,
    };

    private static readonly HashSet<string> Sources = new( Shared.Values, StringComparer.Ordinal );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create( AV0024_InheritedApiExplorerOption );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( Analyze | ReportDiagnostics );
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction( OnCompilationStart );
    }

    private static void OnCompilationStart( CompilationStartAnalysisContext context )
    {
        // nothing shares a value with options that are not there to be configured
        if ( !Symbols.IsReferenced( context.Compilation, Symbols.ApiExplorerOptions ) )
        {
            return;
        }

        var analysis = new Analysis();

        // an object initializer assigns through the same expression as a property does
        context.RegisterSyntaxNodeAction( analysis.OnAssignment, SyntaxKind.SimpleAssignmentExpression );
        context.RegisterCompilationEndAction( analysis.OnCompilationEnd );
    }

    private sealed class Analysis
    {
        private readonly ConcurrentDictionary<string, string?> versioning = new( StringComparer.Ordinal );
        private readonly ConcurrentBag<Assignment> explorer = [];

        public void OnAssignment( SyntaxNodeAnalysisContext context )
        {
            var assignment = (AssignmentExpressionSyntax) context.Node;

            if ( context.SemanticModel.GetSymbolInfo( assignment.Left, context.CancellationToken ).Symbol
                 is not IPropertySymbol property )
            {
                return;
            }

            var declaringType = property.ContainingType;

            if ( Symbols.Declares( declaringType, Symbols.ApiVersioningOptions ) )
            {
                if ( !Sources.Contains( property.Name ) )
                {
                    return;
                }

                var value = Resolve( context, assignment );

                // the same value stated twice is still that value, but two of them disagreeing leaves
                // nothing that can be said about what the API explorer is given
                versioning.AddOrUpdate(
                    property.Name,
                    value,
                    ( _, existing ) => existing == value ? existing : default );
            }
            else if ( Symbols.Declares( declaringType, Symbols.ApiExplorerOptions ) &&
                      Shared.ContainsKey( property.Name ) )
            {
                explorer.Add( new( property.Name, Resolve( context, assignment ), assignment.GetLocation() ) );
            }
        }

        public void OnCompilationEnd( CompilationAnalysisContext context )
        {
            foreach ( var assignment in explorer )
            {
                if ( assignment.Value is null )
                {
                    continue;
                }

                var source = Shared[assignment.Property];
                var inherited = versioning.TryGetValue( source, out var configured )
                              ? configured
                              : Defaults[source];

                if ( inherited == assignment.Value )
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create( AV0024_InheritedApiExplorerOption, assignment.Location ) );
                }
            }
        }

        private static string? Resolve( SyntaxNodeAnalysisContext context, AssignmentExpressionSyntax assignment ) =>
            OptionValue.Resolve( context.SemanticModel, assignment.Right, context.CancellationToken );

        private sealed class Assignment( string property, string? value, Location location )
        {
            public string Property { get; } = property;

            public string? Value { get; } = value;

            public Location Location { get; } = location;
        }
    }
}