// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Net.Http.Headers;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;

internal sealed class ApiVersionPolicyJumpTable : PolicyJumpTable
{
    private readonly bool versionsByUrl;
    private readonly bool versionsByUrlOnly;
    private readonly bool versionsByMediaTypeOnly;
    private readonly RouteDestination rejection;
    private readonly FrozenDictionary<ApiVersion, int> destinations;
    private readonly ApiVersionPolicyFeature? policyFeature;
    private readonly RoutePattern[] routePatterns;
    private readonly IApiVersionParser parser;
    private readonly ApiVersioningOptions options;

    internal ApiVersionPolicyJumpTable(
        RouteDestination rejection,
        FrozenDictionary<ApiVersion, int> destinations,
        ApiVersionPolicyFeature? policyFeature,
        RoutePattern[] routePatterns,
        IApiVersionParser parser,
        IApiVersionParameterSource source,
        ApiVersioningOptions options )
    {
        this.rejection = rejection;
        this.destinations = destinations;
        this.policyFeature = policyFeature;
        this.routePatterns = routePatterns;
        this.parser = parser;
        this.options = options;
        versionsByUrl = routePatterns.Length > 0;
        versionsByUrlOnly = source.VersionsByUrl( allowMultipleLocations: false );
        versionsByMediaTypeOnly = source.VersionsByMediaType( allowMultipleLocations: false );
    }

    public override int GetDestination( HttpContext httpContext )
    {
        var request = httpContext.Request;
        var feature = httpContext.ApiVersioningFeature();
        var apiVersions = new List<string>( capacity: feature.RawRequestedApiVersions.Count + 1 );
        var addedFromUrl = false;

        apiVersions.AddRange( feature.RawRequestedApiVersions );

        if ( versionsByUrl &&
             TryGetApiVersionFromPath( request, out var rawApiVersion ) &&
             DoesNotContainApiVersion( apiVersions, rawApiVersion ) )
        {
            apiVersions.Add( rawApiVersion );
            addedFromUrl = apiVersions.Count == apiVersions.Capacity;
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

                // 2. IApiVersionSelector cannot be used yet because there are no candidates that an
                //    aggregated version model can be computed from to select the 'default' API version
                if ( options.AssumeDefaultVersionWhenUnspecified )
                {
                    return rejection.AssumeDefault;
                }

                // 3. unspecified
                return versionsByUrlOnly
                       /* 404 */ ? rejection.Exit
                       /* 400 */ : rejection.Unspecified;

            case 1:
                rawApiVersion = apiVersions[0];

                if ( !parser.TryParse( rawApiVersion, out var apiVersion ) )
                {
                    if ( versionsByUrl )
                    {
                        feature.RawRequestedApiVersion = rawApiVersion;

                        if ( versionsByUrlOnly )
                        {
                            return rejection.Exit; // 404
                        }
                    }

                    return rejection.Malformed; // 400
                }

                if ( destinations.TryGetValue( apiVersion, out destination ) )
                {
                    return destination;
                }

                httpContext.Features.Set( policyFeature );

                if ( versionsByMediaTypeOnly )
                {
                    if ( request.Headers.ContainsKey( HeaderNames.ContentType ) )
                    {
                        return rejection.UnsupportedMediaType; // 415
                    }

                    return rejection.NotAcceptable; // 406
                }

                return addedFromUrl
                       /* 404 */ ? rejection.Exit
                       /* 400 */ : rejection.Unsupported;
        }

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