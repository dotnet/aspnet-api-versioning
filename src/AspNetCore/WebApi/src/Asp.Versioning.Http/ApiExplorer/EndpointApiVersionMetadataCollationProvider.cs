// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

/// <summary>
/// Represents the API version metadata collection provider for endpoints.
/// </summary>
[CLSCompliant( false )]
public sealed class EndpointApiVersionMetadataCollationProvider : IApiVersionMetadataCollationProvider
{
    private readonly EndpointDataSource endpointDataSource;
    private int version;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointApiVersionMetadataCollationProvider"/> class.
    /// </summary>
    /// <param name="endpointDataSource">The underlying <see cref="endpointDataSource">endpoint data source</see>.</param>
    public EndpointApiVersionMetadataCollationProvider( EndpointDataSource endpointDataSource )
    {
        this.endpointDataSource = endpointDataSource ?? throw new ArgumentNullException( nameof( endpointDataSource ) );
        ChangeToken.OnChange( endpointDataSource.GetChangeToken, () => ++version );
    }

    /// <inheritdoc />
    public int Version => version;

    /// <inheritdoc />
    public void Execute( ApiVersionMetadataCollationContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var endpoints = endpointDataSource.Endpoints;

        for ( var i = 0; i < endpoints.Count; i++ )
        {
            var endpoint = endpoints[i];

            if ( endpoint.Metadata.GetMetadata<ApiVersionMetadata>() is not ApiVersionMetadata item )
            {
                continue;
            }

            var groupName = endpoint.Metadata.OfType<IEndpointGroupNameMetadata>().LastOrDefault()?.EndpointGroupName;
            context.Results.Add( item, groupName );
        }
    }
}