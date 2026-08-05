// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using static Descriptor;

[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class ApiVersionStringSyntaxMustBeValid : StringSyntaxAnalyzer
{
    private const string ApiVersion = nameof( ApiVersion );

    public ApiVersionStringSyntaxMustBeValid()
        : base( ApiVersion, AV0001_InvalidApiVersionSyntax ) { }

    protected override void Validate( string text, Reporter reporter )
    {
        if ( !ApiVersionValidator.IsValid( text ) )
        {
            reporter.Report( AV0001_InvalidApiVersionSyntax );
        }
    }
}