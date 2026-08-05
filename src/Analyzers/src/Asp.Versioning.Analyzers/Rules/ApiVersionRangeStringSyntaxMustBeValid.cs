// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using static Descriptor;

/// <summary>
/// Represents an analyzer that validates an API version range.
/// </summary>
/// <remarks>
/// What a range accepts is decided by the range itself, which is compiled into this assembly rather than described a
/// second time here. A range cannot be asked whether a rule parses without parsing it, so the failure it raises is
/// caught instead; only a compile-time constant reaches this, so it happens where a rule is written and nowhere else.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class ApiVersionRangeStringSyntaxMustBeValid : StringSyntaxAnalyzer
{
    private const string ApiVersionRange = nameof( ApiVersionRange );

    public ApiVersionRangeStringSyntaxMustBeValid()
        : base( ApiVersionRange, AV0002_InvalidApiVersionRangeSyntax ) { }

    protected override void Validate( string text, Reporter reporter )
    {
        try
        {
            Versioning.ApiVersionRange.Parse( text );
        }
        catch ( Exception ex ) when ( ex is FormatException or System.ArgumentException )
        {
            reporter.Report( AV0002_InvalidApiVersionRangeSyntax );
        }
    }
}