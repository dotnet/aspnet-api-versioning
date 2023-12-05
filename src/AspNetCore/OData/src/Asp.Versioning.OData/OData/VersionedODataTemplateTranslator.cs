// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a versioned <see cref="IODataTemplateTranslator">OData template translator</see>.
/// </summary>
[CLSCompliant( false )]
public sealed class VersionedODataTemplateTranslator : IODataTemplateTranslator
{
    /// <inheritdoc />
    public ODataPath? Translate( ODataPathTemplate path, ODataTemplateTranslateContext context )
    {
        ArgumentNullException.ThrowIfNull( path );
        ArgumentNullException.ThrowIfNull( context );

        var apiVersion = context.HttpContext.GetRequestedApiVersion();

        if ( apiVersion == null )
        {
            if ( !IsVersionNeutral( context ) )
            {
                return default;
            }
        }
        else
        {
            var model = context.Model;
            var otherApiVersion = model.GetApiVersion();

            // HACK: a version-neutral endpoint can fail to match here because odata tries to match the
            // first endpoint metadata when there could be multiple. such an endpoint is expected to be
            // the same in all versions so allow it to flow through. revisit if/when odata fixes this.
            //
            // REF: https://github.com/OData/AspNetCoreOData/issues/753
            // REF: https://github.com/OData/AspNetCoreOData/blob/main/src/Microsoft.AspNetCore.OData/Routing/ODataRoutingMatcherPolicy.cs#L86
            if ( !apiVersion.Equals( otherApiVersion ) && !IsVersionNeutral( context ) )
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool IsVersionNeutral( ODataTemplateTranslateContext context ) =>
        context.Endpoint.Metadata.GetMetadata<ApiVersionMetadata>() is ApiVersionMetadata metadata
        && metadata.IsApiVersionNeutral;
}