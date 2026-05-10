// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

internal static class IntroducedInApiVersionStatusCode
{
    internal static bool TryGet( Endpoint endpoint, ApiVersionMetadata metadata, ApiVersion apiVersion, out int statusCode )
    {
        metadata.Deconstruct( out var apiModel, out _ );

        if ( !apiModel.DeclaredApiVersions.Contains( apiVersion ) )
        {
            statusCode = 0;
            return false;
        }

        if ( TryGet( metadata.IntroducedInApiVersions, apiVersion, out statusCode ) )
        {
            return true;
        }

        var endpointMetadata = endpoint.Metadata;

        if ( TryGet( endpointMetadata.GetOrderedMetadata<IntroducedInApiVersionMetadata>(), apiVersion, out statusCode ) )
        {
            return true;
        }

        var reflectedIntroduced = GetIntroducedInApiVersions( endpointMetadata );

        return reflectedIntroduced is not null && TryGet( reflectedIntroduced, apiVersion, out statusCode );
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
        out int statusCode )
    {
        for ( var i = 0; i < introduced.Count; i++ )
        {
            if ( apiVersion < introduced[i].IntroducedIn )
            {
                statusCode = introduced[i].StatusCode;
                return true;
            }
        }

        statusCode = 0;
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
}