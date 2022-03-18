// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.Logging;
using System.Text;

internal sealed class ClientErrorEndpointBuilder
{
    private readonly IApiVersioningFeature feature;
    private readonly CandidateSet candidates;
    private readonly ILogger logger;

    public ClientErrorEndpointBuilder(
        IApiVersioningFeature feature,
        CandidateSet candidates,
        ILogger logger )
    {
        this.feature = feature;
        this.candidates = candidates;
        this.logger = logger;
    }

    public Endpoint Build()
    {
        if ( feature.RawRequestedApiVersions.Count == 0 )
        {
            return new UnspecifiedApiVersionEndpoint( logger, GetDisplayNames() );
        }

        return new UnsupportedApiVersionEndpoint();
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
            return Array.Empty<string>();
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