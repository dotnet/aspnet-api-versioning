// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of an API version parser.
/// </summary>
public interface IApiVersionParser
{
    /// <summary>
    /// Parses the specified text.
    /// </summary>
    /// <param name="text">The text to parse as an API version.</param>
    /// <returns>The parsed API version.</returns>
    ApiVersion Parse( ReadOnlySpan<char> text );

    /// <summary>
    /// Attempts to parse the specified text.
    /// </summary>
    /// <param name="text">The text to parse as an API version.</param>
    /// <param name="apiVersion">The parsed API version or null.</param>
    /// <returns>True if the parsing was successful; otherwise false.</returns>
    bool TryParse( ReadOnlySpan<char> text, [MaybeNullWhen( false )] out ApiVersion apiVersion );
}