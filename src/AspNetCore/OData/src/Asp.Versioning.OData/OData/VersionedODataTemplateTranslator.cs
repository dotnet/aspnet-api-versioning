// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

/// <summary>
/// Represents a versioned <see cref="IODataTemplateTranslator">OData template translator</see>.
/// </summary>
[CLSCompliant( false )]
public sealed class VersionedODataTemplateTranslator : IODataTemplateTranslator
{
    /// <inheritdoc />
    public ODataPath? Translate( ODataPathTemplate path, ODataTemplateTranslateContext context )
    {
        if ( path == null )
        {
            throw new ArgumentNullException( nameof( path ) );
        }

        if ( context == null )
        {
            throw new ArgumentNullException( nameof( context ) );
        }

        var apiVersion = context.HttpContext.GetRequestedApiVersion();

        if ( apiVersion == null )
        {
            var metadata = context.Endpoint.Metadata.GetMetadata<ApiVersionMetadata>();

            if ( metadata == null || !metadata.IsApiVersionNeutral )
            {
                return default;
            }
        }
        else
        {
            var model = context.Model;
            var otherApiVersion = model.GetAnnotationValue<ApiVersionAnnotation>( model )?.ApiVersion;

            if ( !apiVersion.Equals( otherApiVersion ) )
            {
                return default;
            }
        }

        for ( var i = 0; i < path.Count; i++ )
        {
            if ( !path[i].TryTranslate( context ) )
            {
                return default;
            }
        }

        return new( context.Segments );
    }
}