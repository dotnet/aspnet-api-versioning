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

        for ( var i = 0; i < candidates.Count; i++ )
        {
            ref readonly var candidate = ref candidates[i];
            var metadata = candidate.Endpoint.Metadata.GetMetadata<ApiVersionMetadata>();

            if ( metadata is null )
            {
                continue;
            }

            metadata.Deconstruct( out var apiModel, out _ );

            if ( !apiModel.DeclaredApiVersions.Contains( apiVersion ) )
            {
                continue;
            }

            var introduced = metadata.IntroducedInApiVersions;

            for ( var j = 0; j < introduced.Count; j++ )
            {
                if ( apiVersion < introduced[j].IntroducedIn )
                {
                    return introduced[j].StatusCode;
                }
            }

            introduced = candidate.Endpoint.Metadata.GetOrderedMetadata<IntroducedInApiVersionMetadata>();

            for ( var j = 0; j < introduced.Count; j++ )
            {
                if ( apiVersion < introduced[j].IntroducedIn )
                {
                    return introduced[j].StatusCode;
                }
            }

            var reflectedIntroduced = GetIntroducedInApiVersions( candidate.Endpoint.Metadata );

            if ( reflectedIntroduced is null )
            {
                continue;
            }

            for ( var j = 0; j < reflectedIntroduced.Count; j++ )
            {
                if ( apiVersion < reflectedIntroduced[j].IntroducedIn )
                {
                    return reflectedIntroduced[j].StatusCode;
                }
            }
        }

        return 0;
    }

    [UnconditionalSuppressMessage( "ReflectionAnalysis", "IL2075", Justification = "The optional MVC action descriptor metadata is discovered by convention when present." )]
    private static List<IntroducedInApiVersionMetadata>? GetIntroducedInApiVersions( EndpointMetadataCollection metadata )
    {
        var introduced = default( List<IntroducedInApiVersionMetadata> );

        for ( var i = 0; i < metadata.Count; i++ )
        {
            if ( metadata[i] is IIntroducedInApiVersionProvider provider )
            {
                Add( provider, ref introduced );
                continue;
            }

            if ( metadata[i].GetType().GetProperty( "MethodInfo" )?.GetValue( metadata[i] ) is not System.Reflection.MethodInfo method )
            {
                continue;
            }

            foreach ( var attribute in method.GetCustomAttributes( inherit: false ) )
            {
                if ( attribute is IIntroducedInApiVersionProvider introducedProvider )
                {
                    Add( introducedProvider, ref introduced );
                }
            }
        }

        return introduced;
    }

    private static void Add( IIntroducedInApiVersionProvider provider, ref List<IntroducedInApiVersionMetadata>? introduced )
    {
        var versions = provider.Versions;

        introduced ??= [];

        for ( var i = 0; i < versions.Count; i++ )
        {
            introduced.Add( new( versions[i], provider.StatusCode ) );
        }
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