// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

/// <remarks>
/// Describes a problem found in an API version format string. The validator that produces these has no
/// knowledge of diagnostics so that it can mirror the tokenizer it is ported from; mapping a problem
/// onto a descriptor is the analyzer's concern.
/// </remarks>
internal readonly struct FormatProblem
{
    private FormatProblem( FormatProblemKind kind, string specifier, int maxLength, int length )
    {
        Kind = kind;
        Specifier = specifier;
        MaxLength = maxLength;
        Length = length;
    }

    public FormatProblemKind Kind { get; }

    public string Specifier { get; }

    public int MaxLength { get; }

    public int Length { get; }

    public static FormatProblem UnterminatedLiteral( char delimiter ) =>
        new( FormatProblemKind.UnterminatedLiteral, delimiter.ToString(), 0, 0 );

    public static FormatProblem PaddingOutOfRange( string count ) =>
        new( FormatProblemKind.PaddingOutOfRange, count, ApiVersionFormatProvider.MaxPadding, 0 );

    public static FormatProblem RepeatedSpecifier( string text, char specifier, int maxLength, int length ) =>
        new( FormatProblemKind.RepeatedSpecifier, specifier.ToString(), maxLength, length );
}