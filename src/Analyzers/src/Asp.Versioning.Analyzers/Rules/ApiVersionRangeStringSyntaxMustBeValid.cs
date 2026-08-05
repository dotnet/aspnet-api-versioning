// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using static Descriptor;

[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class ApiVersionRangeStringSyntaxMustBeValid : StringSyntaxAnalyzer
{
    private const string ApiVersionRange = nameof( ApiVersionRange );

    public ApiVersionRangeStringSyntaxMustBeValid()
        : base( ApiVersionRange, AV0002_InvalidApiVersionRangeSyntax ) { }

    protected override void Validate( string text, Reporter reporter )
    {
        if ( !ApiVersionRangeValidator.IsValid( text ) )
        {
            reporter.Report( AV0002_InvalidApiVersionRangeSyntax );
        }
    }
}