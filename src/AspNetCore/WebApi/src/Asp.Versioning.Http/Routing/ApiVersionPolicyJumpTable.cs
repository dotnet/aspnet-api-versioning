// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Net.Http.Headers;
using System.Runtime.CompilerServices;

internal sealed class ApiVersionPolicyJumpTable : PolicyJumpTable
{
    private readonly bool versionsByUrl;
    private readonly bool versionsByMediaTypeOnly;
    private readonly RouteDestination rejection;
    private readonly IReadOnlyDictionary<ApiVersion, int> destinations;
    private readonly IReadOnlyList<RoutePattern> routePatterns;
    private readonly IApiVersionParser parser;
    private readonly ApiVersioningOptions options;

    internal ApiVersionPolicyJumpTable(
        RouteDestination rejection,
        IReadOnlyDictionary<ApiVersion, int> destinations,
        IReadOnlyList<RoutePattern> routePatterns,
        IApiVersionParser parser,
        IApiVersionParameterSource source,
        ApiVersioningOptions options )
    {
        this.rejection = rejection;
        this.destinations = destinations;
        this.routePatterns = routePatterns;
        this.parser = parser;
        this.options = options;
        versionsByUrl = routePatterns.Count > 0;
        versionsByMediaTypeOnly = source.VersionsByMediaType( allowMultipleLocations: false );
    }

    public override int GetDestination( HttpContext httpContext )
    {
        var request = httpContext.Request;
        var feature = httpContext.ApiVersioningFeature();
        var apiVersions = new List<string>( capacity: feature.RawRequestedApiVersions.Count + 1 );

        apiVersions.AddRange( feature.RawRequestedApiVersions );

        if ( versionsByUrl &&
             TryGetApiVersionFromPath( request, out var rawApiVersion ) &&
             DoesNotContainApiVersion( apiVersions, rawApiVersion ) )
        {
            apiVersions.Add( rawApiVersion );
        }

        int destination;

        switch ( apiVersions.Count )
        {
            case 0:
                // 1. version-neutral endpoints take precedence
                if ( destinations.TryGetValue( ApiVersion.Neutral, out destination ) )
                {
                    return destination;
                }

                // 2. short-circuit if a default version cannot be assumed
                if ( !options.AssumeDefaultVersionWhenUnspecified )
                {
                    return rejection.Unspecified; // 400
                }

                // 3. IApiVersionSelector cannot be used yet because there are no candidates that an
                //    aggregated version model can be computed from to select the 'default' API version
                return rejection.AssumeDefault;
            case 1:
                rawApiVersion = apiVersions[0];

                if ( !parser.TryParse( rawApiVersion, out var apiVersion ) )
                {
                    if ( versionsByUrl )
                    {
                        feature.RawRequestedApiVersion = rawApiVersion;
                    }

                    return rejection.Malformed; // 400
                }

                if ( destinations.TryGetValue( apiVersion, out destination ) )
                {
                    return destination;
                }

                if ( versionsByMediaTypeOnly )
                {
                    if ( request.Headers.ContainsKey( HeaderNames.ContentType ) )
                    {
                        return rejection.UnsupportedMediaType; // 415
                    }

                    return rejection.NotAcceptable; // 406
                }

                return rejection.Exit; // 404
        }

        var addedFromUrl = apiVersions.Count == apiVersions.Capacity;

        if ( addedFromUrl )
        {
            feature.RawRequestedApiVersions = apiVersions;
        }

        return rejection.Ambiguous; // 400
    }

    private bool DoesNotContainApiVersion( List<string> apiVersions, string otherRawApiVersion )
    {
        for ( var i = 0; i < apiVersions.Count; i++ )
        {
            var rawApiVersion = apiVersions[i];

            if ( rawApiVersion.Equals( otherRawApiVersion, StringComparison.OrdinalIgnoreCase ) ||
                 AreEquivalentSlow( rawApiVersion, otherRawApiVersion ) )
            {
                return false;
            }
        }

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool AreEquivalentSlow( string rawApiVersion, string otherRawApiVersion ) =>
        parser.TryParse( rawApiVersion, out var apiVersion ) &&
        parser.TryParse( otherRawApiVersion, out var otherApiVersion ) &&
        apiVersion.Equals( otherApiVersion );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool TryGetApiVersionFromPath( HttpRequest request, [NotNullWhen( true )] out string? apiVersion ) =>
        request.TryGetApiVersionFromPath( routePatterns, options.RouteConstraintName, out apiVersion );
}