// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="IApiVersionParser"/> interface.
/// </summary>
public static class IApiVersionParserExtensions
{
    /// <summary>
    /// Parses the specified text.
    /// </summary>
    /// <param name="parser">The extended parser.</param>
    /// <param name="text">The text to parse as an API version.</param>
    /// <returns>The parsed API version.</returns>
    public static ApiVersion Parse( this IApiVersionParser parser, string? text )
    {
        ArgumentNullException.ThrowIfNull( parser );
        return parser.Parse( text == null ? default : text.AsSpan() );
    }

    /// <summary>
    /// Attempts to parse the specified text.
    /// </summary>
    /// <param name="parser">The extended parser.</param>
    /// <param name="text">The text to parse as an API version.</param>
    /// <param name="apiVersion">The parsed API version or null.</param>
    /// <returns>True if the parsing was successful; otherwise false.</returns>
    public static bool TryParse(
        this IApiVersionParser parser,
        string? text,
        [MaybeNullWhen( false )] out ApiVersion apiVersion )
    {
        ArgumentNullException.ThrowIfNull( parser );
        return parser.TryParse( text == null ? default : text.AsSpan(), out apiVersion );
    }
}