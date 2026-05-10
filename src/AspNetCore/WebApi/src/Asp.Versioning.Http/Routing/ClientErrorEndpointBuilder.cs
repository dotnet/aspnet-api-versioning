// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.Logging;

internal sealed class ClientErrorEndpointBuilder
{
    private readonly IApiVersioningFeature feature;
    private readonly CandidateSet candidates;
    private readonly ApiVersioningOptions options;
    private readonly ILogger logger;

    public ClientErrorEndpointBuilder(
        IApiVersioningFeature feature,
        CandidateSet candidates,
        ApiVersioningOptions options,
        ILogger logger )
    {
        this.feature = feature;
        this.candidates = candidates;
        this.options = options;
        this.logger = logger;
    }

    public Endpoint Build()
    {
        if ( feature.RawRequestedApiVersions.Count == 0 )
        {
            return new UnspecifiedApiVersionEndpoint( logger, options, GetDisplayNames() );
        }

        var introducedInApiVersionStatusCode = GetIntroducedInApiVersionStatusCode();

        if ( introducedInApiVersionStatusCode > 0 )
        {
            return new IntroducedInApiVersionEndpoint( introducedInApiVersionStatusCode );
        }

        return new UnsupportedApiVersionEndpoint( options );
    }

    private int GetIntroducedInApiVersionStatusCode()
    {
        var apiVersion = feature.RequestedApiVersion;

        if ( apiVersion is null )
        {
            return 0;
        }

        var result = 0;

        for ( var i = 0; i < candidates.Count; i++ )
        {
            ref readonly var candidate = ref candidates[i];
            var metadata = candidate.Endpoint.Metadata.GetMetadata<ApiVersionMetadata>();

            if ( metadata is null )
            {
                continue;
            }

            if ( IntroducedInApiVersionStatusCode.TryGet(
                candidate.Endpoint,
                metadata,
                apiVersion,
                options.UnsupportedApiVersionStatusCode,
                out var statusCode ) )
            {
                if ( result == 0 || statusCode < result )
                {
                    result = statusCode;
                }
            }
        }

        return result;
    }

    private static string DisplayName( Endpoint endpoint )
    {
        var displayName = endpoint.DisplayName;

        if ( string.IsNullOrEmpty( displayName ) && endpoint is RouteEndpoint route )
        {
            displayName = route.RoutePattern.RawText;
        }

        if ( string.IsNullOrEmpty( displayName ) )
        {
            displayName = "(null)";
        }

        return displayName;
    }

    private string[] GetDisplayNames()
    {
        if ( candidates.Count == 0 )
        {
            return [];
        }

        ref readonly var candidate = ref candidates[0];
        var displayNames = new string[candidates.Count];

        displayNames[0] = DisplayName( candidate.Endpoint );

        for ( var i = 1; i < candidates.Count; i++ )
        {
            candidate = ref candidates[i];
            displayNames[i] = DisplayName( candidate.Endpoint );
        }

        return displayNames;
    }
}