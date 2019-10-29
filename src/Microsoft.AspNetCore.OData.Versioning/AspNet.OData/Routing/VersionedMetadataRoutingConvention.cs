namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
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

            return odataPath.PathTemplate == "~" || odataPath.PathTemplate == "~/$metadata" ? "VersionedMetadata" : null;
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
            var actionCollectionProvider = httpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
            var odataPath = httpContext.ODataFeature().Path;
            var request = httpContext.Request;
            var controller = SelectController( odataPath, request );

            if ( controller == null )
            {
                return NoActions;
            }

            var actionDescriptors = actionCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>().Where( c => c.ControllerName == controller );

            if ( odataPath.PathTemplate == "~" )
            {
                return actionDescriptors.Where( a => a.ActionName == nameof( VersionedMetadataController.GetServiceDocument ) );
            }

            if ( odataPath.PathTemplate != "~/$metadata" )
            {
                return NoActions;
            }

            var method = request.Method;

            if ( method.Equals( "GET", OrdinalIgnoreCase ) )
            {
                return actionDescriptors.Where( a => a.ActionName == nameof( VersionedMetadataController.GetMetadata ) );
            }
            else if ( method.Equals( "OPTIONS", OrdinalIgnoreCase ) )
            {
                return actionDescriptors.Where( a => a.ActionName == nameof( VersionedMetadataController.GetOptions ) );
            }

            return NoActions;
        }
    }
}