// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using static Descriptor;

/// <summary>
/// Represents an analyzer that validates an API version.
/// </summary>
/// <remarks>
/// What an API version accepts is decided by the parser that reads one, which is compiled into this assembly rather
/// than described a second time here. A value the parser rejects is a value that throws where it is read.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class ApiVersionStringSyntaxMustBeValid : StringSyntaxAnalyzer
{
    private const string ApiVersion = nameof( ApiVersion );

    public ApiVersionStringSyntaxMustBeValid()
        : base( ApiVersion, AV0001_InvalidApiVersionSyntax ) { }

    protected override void Validate( string text, Reporter reporter )
    {
        if ( !ApiVersionParser.Default.TryParse( text.AsSpan(), out _ ) )
        {
            reporter.Report( AV0001_InvalidApiVersionSyntax );
        }
    }
}