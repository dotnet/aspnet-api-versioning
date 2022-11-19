// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Routing.Patterns;
using System.Diagnostics.CodeAnalysis;
using static Asp.Versioning.Routing.EndpointType;

internal readonly struct EdgeKey : IEquatable<EdgeKey>
{
    public readonly ApiVersion ApiVersion;
    public readonly ApiVersionMetadata Metadata;
    public readonly HashSet<RoutePattern> RoutePatterns;
    public readonly EndpointType EndpointType;

    private EdgeKey( EndpointType endpointType, HashSet<RoutePattern> routePatterns )
    {
        ApiVersion = ApiVersion.Default;
        Metadata = ApiVersionMetadata.Empty;
        RoutePatterns = routePatterns;
        EndpointType = endpointType;
    }

    internal EdgeKey(
        ApiVersion apiVersion,
        ApiVersionMetadata metadata,
        HashSet<RoutePattern> routePatterns )
    {
        ApiVersion = apiVersion;
        Metadata = metadata;
        RoutePatterns = routePatterns;
        EndpointType = UserDefined;
    }

    internal static EdgeKey Ambiguous => new( EndpointType.Ambiguous, Set.Empty );

    internal static EdgeKey Malformed => new( EndpointType.Malformed, Set.Empty );

    internal static EdgeKey Unspecified => new( EndpointType.Unspecified, Set.Empty );

    internal static EdgeKey Unsupported => new( EndpointType.Unsupported, Set.Empty );

    internal static EdgeKey UnsupportedMediaType => new( EndpointType.UnsupportedMediaType, Set.Empty );

    internal static EdgeKey NotAcceptable => new( EndpointType.NotAcceptable, Set.Empty );

    internal static EdgeKey AssumeDefault => new( EndpointType.AssumeDefault, new( new RoutePatternComparer() ) );

    public bool Equals( [AllowNull] EdgeKey other ) => GetHashCode() == other.GetHashCode();

    public override bool Equals( object? obj ) => obj is EdgeKey other && Equals( other );

    public override int GetHashCode()
    {
        var result = default( HashCode );

        result.Add( EndpointType );

        if ( EndpointType == UserDefined )
        {
            result.Add( ApiVersion );
        }

        return result.ToHashCode();
    }

    public override string ToString()
    {
        string value;

        if ( ApiVersion == ApiVersion.Neutral )
        {
            value = "*";
        }
        else if ( EndpointType == UserDefined )
        {
            value = ApiVersion.ToString();
        }
        else
        {
            value = EndpointType.ToString();
        }

        return "VER: " + value;
    }

    private static class Set
    {
        public static readonly HashSet<RoutePattern> Empty = new( capacity: 0 );
    }
}