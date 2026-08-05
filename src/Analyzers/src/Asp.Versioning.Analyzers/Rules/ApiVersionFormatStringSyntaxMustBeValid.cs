// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Asp.Versioning.Analyzers;

using static Descriptor;

/// <summary>
/// Represents an analyzer that validates an API version format.
/// </summary>
/// <remarks>
/// Whether a format is valid is decided by applying it to an API version, which is what it will be used for. The
/// version applied to declares every component so that each specifier resolves to something; a format that fails
/// does so because of the format rather than what it was given.
/// </remarks>
[DiagnosticAnalyzer( LanguageNames.CSharp )]
public sealed class ApiVersionFormatStringSyntaxMustBeValid : StringSyntaxAnalyzer
{
    private const string ApiVersionFormat = nameof( ApiVersionFormat );

    private static readonly ApiVersion Sample = ApiVersionParser.Default.Parse( "2000-01-01.1.1-alpha".AsSpan() );

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

        if ( !CanBeApplied( text, out var reason ) )
        {
            reporter.Report( AV0009_InvalidApiVersionFormat, Explain( problems, reason ) );
        }

        // a repeated specifier is applied rather than rejected, so it is only ever reported as unexpected
        foreach ( var problem in problems )
        {
            if ( problem.Kind == FormatProblemKind.RepeatedSpecifier )
            {
                reporter.Report(
                    AV0010_UnexpectedApiVersionFormat,
                    problem.Specifier,
                    problem.MaxLength,
                    problem.Length );
            }
        }
    }

    private static bool CanBeApplied( string format, out string reason )
    {
        try
        {
            Sample.ToString( format );
        }
        catch ( FormatException ex )
        {
            reason = ex.Message;
            return false;
        }

        reason = string.Empty;
        return true;
    }

    /// <remarks>The failure names no part of the format, so what was found in it is preferred when there is
    /// something to say; the failure is what remains when there is not.</remarks>
    private static string Explain( List<FormatProblem> problems, string reason )
    {
        foreach ( var problem in problems )
        {
            switch ( problem.Kind )
            {
                case FormatProblemKind.UnterminatedLiteral:
                    return $"The literal delimited by {problem.Specifier} is not terminated.";
                case FormatProblemKind.PaddingOutOfRange:
                    return $"The padding count '{problem.Specifier}' must be between 0 and {problem.MaxLength}.";
            }
        }

        return reason;
    }
}