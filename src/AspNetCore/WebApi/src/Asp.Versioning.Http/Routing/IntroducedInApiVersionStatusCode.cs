// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

internal static class IntroducedInApiVersionStatusCode
{
    internal static bool TryGet(
        Endpoint endpoint,
        ApiVersionMetadata metadata,
        ApiVersion apiVersion,
        int unsupportedApiVersionStatusCode,
        out int statusCode ) =>
        TryGet( endpoint, metadata, apiVersion, unsupportedApiVersionStatusCode, out statusCode, out _ );

    internal static bool TryGet(
        Endpoint endpoint,
        ApiVersionMetadata metadata,
        ApiVersion apiVersion,
        int unsupportedApiVersionStatusCode,
        out int statusCode,
        [NotNullWhen( true )] out ApiVersion? introducedIn )
    {
        metadata.Deconstruct( out var apiModel, out _ );

        if ( !apiModel.DeclaredApiVersions.Contains( apiVersion ) )
        {
            statusCode = 0;
            introducedIn = default;
            return false;
        }

        if ( TryGet( metadata.IntroducedInApiVersions, apiVersion, unsupportedApiVersionStatusCode, out statusCode, out introducedIn ) )
        {
            return true;
        }

        var endpointMetadata = endpoint.Metadata;

        if ( TryGet( endpointMetadata.GetOrderedMetadata<IntroducedInApiVersionMetadata>(), apiVersion, unsupportedApiVersionStatusCode, out statusCode, out introducedIn ) )
        {
            return true;
        }

        var reflectedIntroduced = GetIntroducedInApiVersions( endpointMetadata );

        if ( reflectedIntroduced is not null &&
             TryGet( reflectedIntroduced, apiVersion, unsupportedApiVersionStatusCode, out statusCode, out introducedIn ) )
        {
            return true;
        }

        statusCode = 0;
        introducedIn = default;
        return false;
    }

    internal static bool HasIntroducedInApiVersion( Endpoint endpoint, ApiVersionMetadata metadata )
    {
        if ( metadata.IntroducedInApiVersions.Count > 0 ||
             endpoint.Metadata.GetOrderedMetadata<IntroducedInApiVersionMetadata>().Count > 0 )
        {
            return true;
        }

        return GetIntroducedInApiVersions( endpoint.Metadata ) is { Count: > 0 };
    }

    private static bool TryGet(
        IReadOnlyList<IntroducedInApiVersionMetadata> introduced,
        ApiVersion apiVersion,
        int unsupportedApiVersionStatusCode,
        out int statusCode,
        [NotNullWhen( true )] out ApiVersion? introducedIn )
    {
        var matched = default( IntroducedInApiVersionMetadata );
        var matchedStatusCode = 0;

        for ( var i = 0; i < introduced.Count; i++ )
        {
            var current = introduced[i];
            var currentStatusCode = current.StatusCode;

            if ( currentStatusCode == IntroducedInApiVersionAttribute.UseConfiguredStatusCode )
            {
                currentStatusCode = unsupportedApiVersionStatusCode;
            }

            if ( apiVersion < current.IntroducedIn &&
                 ( matched is null ||
                   current.IntroducedIn > matched.IntroducedIn ||
                   ( current.IntroducedIn == matched.IntroducedIn && currentStatusCode < matchedStatusCode ) ) )
            {
                matched = current;
                matchedStatusCode = currentStatusCode;
            }
        }

        if ( matched is not null )
        {
            statusCode = matchedStatusCode;
            introducedIn = matched.IntroducedIn;
            return true;
        }

        statusCode = 0;
        introducedIn = default;
        return false;
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

            foreach ( var introducedProvider in method.GetCustomAttributes( inherit: false ).OfType<IIntroducedInApiVersionProvider>() )
            {
                Add( introducedProvider, ref introduced );
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
}