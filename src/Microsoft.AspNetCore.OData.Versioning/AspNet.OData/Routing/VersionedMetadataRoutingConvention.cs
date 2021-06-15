namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static System.StringComparison;

    /// <summary>
    /// Represents the <see cref="IODataRoutingConvention">OData routing convention</see> for versioned service and metadata documents.
    /// </summary>
    [CLSCompliant( false )]
    public class VersionedMetadataRoutingConvention : IODataRoutingConvention
    {
        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="request">The request.</param>
        /// <returns>The name of the selected controller or <c>null</c> if the request isn't handled by this convention.</returns>
        protected virtual string? SelectController( ODataPath odataPath, HttpRequest request )
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

            var context = request.HttpContext;
            var feature = context.ApiVersioningFeature();
            string? apiVersion;

            try
            {
                apiVersion = feature.RawRequestedApiVersion;
            }
            catch ( AmbiguousApiVersionException )
            {
                // the appropriate response will be handled by policy
                return "VersionedMetadata";
            }

            // the service document and metadata endpoints are special, but they are not neutral. if the client doesn't
            // specify a version, they may not know to. assume a default version by policy, but it's always allowed.
            // a client might also send an OPTIONS request to determine which versions are available (ex: tooling)
            if ( string.IsNullOrEmpty( apiVersion ) )
            {
                var modelSelector = request.GetRequestContainer().GetRequiredService<IEdmModelSelector>();
                var versionSelector = context.RequestServices.GetRequiredService<IApiVersionSelector>();
                var model = new ApiVersionModel( modelSelector.ApiVersions, Enumerable.Empty<ApiVersion>() );

                feature.RequestedApiVersion = versionSelector.SelectVersion( request, model );
            }

            return "VersionedMetadata";
        }

        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="routeContext">The route context.</param>
        /// <returns>The name of the selected action or <c>null</c> if the request isn't handled by this convention.</returns>
        /// <remarks>The matching <see cref="IEnumerable{T}">sequence</see> of <see cref="ControllerActionDescriptor">actions</see>
        /// or <c>null</c> if no actions match the convention.</remarks>
        public virtual IEnumerable<ControllerActionDescriptor>? SelectAction( RouteContext routeContext )
        {
            if ( routeContext == null )
            {
                throw new ArgumentNullException( nameof( routeContext ) );
            }

            const IEnumerable<ControllerActionDescriptor>? NoActions = default;
            var httpContext = routeContext.HttpContext;
            var odataPath = httpContext.ODataFeature().Path;
            var request = httpContext.Request;
            var controller = SelectController( odataPath, request );

            if ( controller == null )
            {
                return NoActions;
            }

            var actionCollectionProvider = httpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
            var actions = actionCollectionProvider.ActionDescriptors.Items;

            if ( odataPath.PathTemplate == "~" )
            {
                return SelectActions( actions, controller, nameof( VersionedMetadataController.GetServiceDocument ) );
            }

            if ( odataPath.PathTemplate != "~/$metadata" )
            {
                return NoActions;
            }

            var method = request.Method;

            if ( method.Equals( "GET", OrdinalIgnoreCase ) )
            {
                return SelectActions( actions, controller, nameof( VersionedMetadataController.GetMetadata ) );
            }
            else if ( method.Equals( "OPTIONS", OrdinalIgnoreCase ) )
            {
                return SelectActions( actions, controller, nameof( VersionedMetadataController.GetOptions ) );
            }

            return NoActions;
        }

        static IEnumerable<ControllerActionDescriptor> SelectActions(
            IReadOnlyList<ActionDescriptor> actions,
            string controllerName,
            string actionName )
        {
            for ( var i = 0; i < actions.Count; i++ )
            {
                if ( actions[i] is ControllerActionDescriptor action )
                {
                    if ( action.ControllerName == controllerName && action.ActionName == actionName )
                    {
                        yield return action;
                    }
                }
            }
        }
    }
}