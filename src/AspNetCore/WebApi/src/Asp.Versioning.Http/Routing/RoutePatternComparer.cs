// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using RoutePattern = Microsoft.AspNetCore.Routing.Patterns.RoutePattern;

/// <summary>
/// Represents a comparer for comparing <see cref="RoutePattern"/> instances.
/// </summary>
[CLSCompliant( false )]
public sealed class RoutePatternComparer : IEqualityComparer<RoutePattern>
{
    private readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;

    /// <inheritdoc />
    public bool Equals( [AllowNull] RoutePattern x, [AllowNull] RoutePattern y )
    {
        if ( x is null )
        {
            return y is null;
        }

        if ( y is null )
        {
            return false;
        }

        return comparer.Equals( x.RawText, y.RawText );
    }

    /// <inheritdoc />
    public int GetHashCode( [DisallowNull] RoutePattern obj ) =>
        obj?.RawText is string text ? comparer.GetHashCode( text ) : 0;
}