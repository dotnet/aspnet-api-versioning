// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

/// <summary>
/// Determines whether a namespace declares an API version.
/// </summary>
/// <remarks>
/// What an identifier has to look like to declare a version is decided by the parser that reads one, which is
/// compiled into this assembly and reached by deriving from it. The parser reads the namespace of a type, which an
/// analyzer has as text rather than as a type, so the parts are walked here and each is handed to the parser. Only
/// whether a version was found matters, not which one, so the parsed value is discarded.
/// </remarks>
public static class NamespaceVersion
{
    /// <summary>
    /// Determines whether any part of a namespace declares an API version.
    /// </summary>
    /// <param name="namespace">The namespace to evaluate.</param>
    /// <returns>True if any part of the <paramref name="namespace"/> declares an API version;
    /// otherwise, false.</returns>
    public static bool IsVersioned( string? @namespace )
    {
        if ( string.IsNullOrEmpty( @namespace ) )
        {
            return false;
        }

        var start = 0;

        for ( var end = 0; end <= @namespace!.Length; end++ )
        {
            if ( end < @namespace.Length && @namespace[end] != '.' )
            {
                continue;
            }

            if ( Identifier.Parser.IsVersion( @namespace.Substring( start, end - start ) ) )
            {
                return true;
            }

            start = end + 1;
        }

        return false;
    }

    private sealed class Identifier : NamespaceParser
    {
        internal static readonly Identifier Parser = new();

        internal bool IsVersion( string identifier ) => TryParse( identifier, out var version ) && version is not null;
    }
}