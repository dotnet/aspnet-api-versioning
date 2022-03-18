// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Asp.Versioning.Controllers;
using Asp.Versioning.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using static System.Net.Http.HttpMethod;

/// <summary>
/// Represents the <see cref="IODataRoutingConvention">OData routing convention</see> for versioned service and metadata documents.
/// </summary>
public class VersionedMetadataRoutingConvention : IODataRoutingConvention
{
    /// <summary>
    /// Selects the controller for OData requests.
    /// </summary>
    /// <param name="odataPath">The OData path.</param>
    /// <param name="request">The request.</param>
    /// <returns>The name of the selected controller or <c>null</c> if the request isn't handled by this convention.</returns>
    public virtual string? SelectController( ODataPath odataPath, HttpRequestMessage request )
    {
        if ( odataPath == null )
        {
            throw new ArgumentNullException( nameof( odataPath ) );
        }

        if ( request == null )
        {
            throw new ArgumentNullException( nameof( request ) );
        }

        if ( odataPath.PathTemplate != "~" && odataPath.PathTemplate != "~/$metadata" )
        {
            return null;
        }

        var properties = request.ApiVersionProperties();

        // the service document and metadata endpoints are special, but they are not neutral. if the client doesn't
        // specify a version, they may not know to. assume a default version by policy, but it's always allowed.
        // a client might also send an OPTIONS request to determine which versions are available (ex: tooling)
        if ( properties.RawRequestedApiVersions.Count == 0 )
        {
            var modelSelector = request.GetRequestContainer().GetRequiredService<IEdmModelSelector>();
            var versionSelector = request.GetApiVersioningOptions().ApiVersionSelector;
            var model = new ApiVersionModel( modelSelector.ApiVersions, Enumerable.Empty<ApiVersion>() );

            properties.RequestedApiVersion = versionSelector.SelectVersion( request, model );
        }

        return "VersionedMetadata";
    }

    /// <summary>
    /// Selects the action for OData requests.
    /// </summary>
    /// <param name="odataPath">The OData path.</param>
    /// <param name="controllerContext">The controller context.</param>
    /// <param name="actionMap">The action map.</param>
    /// <returns>The name of the selected action or <c>null</c> if the request isn't handled by this convention.</returns>
    public virtual string? SelectAction(
        ODataPath odataPath,
        HttpControllerContext controllerContext,
        ILookup<string, HttpActionDescriptor> actionMap )
    {
        if ( odataPath == null )
        {
            throw new ArgumentNullException( nameof( odataPath ) );
        }

        if ( controllerContext == null )
        {
            throw new ArgumentNullException( nameof( controllerContext ) );
        }

        if ( actionMap == null )
        {
            throw new ArgumentNullException( nameof( actionMap ) );
        }

        if ( odataPath.PathTemplate == "~" )
        {
            return nameof( VersionedMetadataController.GetServiceDocument );
        }

        if ( odataPath.PathTemplate != "~/$metadata" )
        {
            return null;
        }

        var method = controllerContext.Request.Method;

        if ( method == Get )
        {
            return nameof( VersionedMetadataController.GetMetadata );
        }
        else if ( method == Options )
        {
            return nameof( VersionedMetadataController.GetOptions );
        }

        return null;
    }
}