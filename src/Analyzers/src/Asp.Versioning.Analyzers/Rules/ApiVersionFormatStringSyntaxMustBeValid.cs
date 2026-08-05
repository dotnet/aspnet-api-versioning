// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using static Descriptor;

[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class ApiVersionFormatStringSyntaxMustBeValid : StringSyntaxAnalyzer
{
    private const string ApiVersionFormat = nameof( ApiVersionFormat );

    public ApiVersionFormatStringSyntaxMustBeValid()
        : base( ApiVersionFormat, AV0009_InvalidApiVersionFormat, AV0010_UnexpectedApiVersionFormat ) { }

    protected override void Validate( string text, Reporter reporter )
    {
        // an empty format is the full format, so there is nothing to validate
        if ( text.Length == 0 )
        {
            return;
        }

        var problems = new List<FormatProblem>();

        ApiVersionFormatValidator.Validate( text, problems );

        foreach ( var problem in problems )
        {
            switch ( problem.Kind )
            {
                case FormatProblemKind.UnterminatedLiteral:
                    reporter.Report(
                        AV0009_InvalidApiVersionFormat,
                        $"The literal delimited by {problem.Specifier} is not terminated." );
                    break;
                case FormatProblemKind.PaddingOutOfRange:
                    reporter.Report(
                        AV0009_InvalidApiVersionFormat,
                        $"The padding count '{problem.Specifier}' must be between 0 and {problem.MaxLength}." );
                    break;
                case FormatProblemKind.RepeatedSpecifier:
                    reporter.Report(
                        AV0010_UnexpectedApiVersionFormat,
                        problem.Specifier,
                        problem.MaxLength,
                        problem.Length );
                    break;
            }
        }
    }
}