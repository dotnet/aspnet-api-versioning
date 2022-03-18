// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Routing.Patterns;
using System.Diagnostics.CodeAnalysis;
using static Asp.Versioning.Routing.EndpointType;

internal readonly struct EdgeKey : IEquatable<EdgeKey>
{
    public readonly ApiVersion ApiVersion;
    public readonly List<RoutePattern> RoutePatterns;
    public readonly EndpointType EndpointType;

    private EdgeKey( EndpointType endpointType, List<RoutePattern> routePatterns )
    {
        ApiVersion = ApiVersion.Default;
        RoutePatterns = routePatterns;
        EndpointType = endpointType;
    }

    internal EdgeKey( ApiVersion apiVersion )
    {
        ApiVersion = apiVersion;
        RoutePatterns = new();
        EndpointType = UserDefined;
    }

    internal static EdgeKey Ambiguous => new( EndpointType.Ambiguous, new( capacity: 0 ) );

    internal static EdgeKey Malformed => new( EndpointType.Malformed, new( capacity: 0 ) );

    internal static EdgeKey Unspecified => new( EndpointType.Unspecified, new( capacity: 0 ) );

    internal static EdgeKey UnsupportedMediaType => new( EndpointType.UnsupportedMediaType, new( capacity: 0 ) );

    internal static EdgeKey AssumeDefault => new( EndpointType.AssumeDefault, new() );

    public bool Equals( [AllowNull] EdgeKey other ) => GetHashCode() == other.GetHashCode();

    public override bool Equals( object? obj ) => obj is EdgeKey other && Equals( other );

    public override int GetHashCode() =>
        EndpointType == UserDefined ?
        HashCode.Combine( ApiVersion, EndpointType ) :
        EndpointType.GetHashCode();

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
}