namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNet.OData.Routing.Template;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class VersionedAttributeRoutingConvention : IODataRoutingConvention
    {
        readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider">HTTP configuration</see>.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the convention.</param>
        public VersionedAttributeRoutingConvention( string routeName, IServiceProvider serviceProvider, ApiVersion apiVersion )
        {
            if ( serviceProvider == null )
            {
                throw new ArgumentNullException( nameof( serviceProvider ) );
            }

            var perRouteContainer = serviceProvider.GetRequiredService<IPerRouteContainer>();
            var rootContainer = perRouteContainer.GetODataRootContainer( routeName );

            this.routeName = routeName;
            this.serviceProvider = serviceProvider;
            ApiVersion = apiVersion;
            ODataPathTemplateHandler = rootContainer.GetRequiredService<IODataPathTemplateHandler>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider">HTTP configuration</see>.</param>
        /// <param name="pathTemplateHandler">The <see cref="IODataPathTemplateHandler">OData path template handler</see> associated with the routing convention.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the convention.</param>
        public VersionedAttributeRoutingConvention( string routeName, IServiceProvider serviceProvider, IODataPathTemplateHandler pathTemplateHandler, ApiVersion apiVersion )
        {
            this.routeName = routeName;
            this.serviceProvider = serviceProvider;
            ApiVersion = apiVersion;
            ODataPathTemplateHandler = pathTemplateHandler;
        }

        IDictionary<ODataPathTemplate, ControllerActionDescriptor> AttributeMappings
        {
            get
            {
                if ( attributeMappings == null )
                {
                    var provider = serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();
                    var actions = provider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>();
                    attributeMappings = BuildAttributeMappings( actions );
                }

                return attributeMappings;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified action should be mapped using attribute routing conventions.
        /// </summary>
        /// <param name="action">The <see cref="ControllerActionDescriptor">controller action descriptor</see> to evaluate.</param>
        /// <returns>True if the <paramref name="action"/> should be mapped as an OData action or function; otherwise, false.</returns>
        /// <remarks>This method will match any OData action that explicitly or implicitly matches the API version applied
        /// to the associated <see cref="ApiVersionModel">model</see>.</remarks>
        public virtual bool ShouldMapAction( ControllerActionDescriptor action ) => action.IsMappedTo( ApiVersion );

        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="routeContext">The current <see cref="RouteContext">context</see>.</param>
        /// <returns>The name of the selected controller; otherwise, <c>null</c> if the request isn't handled by this convention.</returns>
        protected virtual IEnumerable<SelectControllerResult> SelectController( RouteContext routeContext )
        {
            if ( routeContext == null )
            {
                throw new ArgumentNullException( nameof( routeContext ) );
            }

            var items = new Dictionary<string, object>();
            var feature = routeContext.HttpContext.ODataFeature();
            var odataPath = feature.Path;
            IDictionary<string, object> routeData = routeContext.RouteData.Values;

            foreach ( var attributeMapping in AttributeMappings )
            {
                var template = attributeMapping.Key;
                var action = attributeMapping.Value;

                if ( !template.TryMatch( odataPath, items ) )
                {
                    continue;
                }

                foreach ( var item in items )
                {
                    if ( IsODataRouteParameter( item ) )
                    {
                        feature.RoutingConventionsStore[item.Key] = item.Value;
                    }
                    else
                    {
                        routeData[item.Key] = item.Value;
                    }
                }

                items[ODataRouteConstants.Action] = action.ActionName;

                yield return new SelectControllerResult( action.ControllerName, items );
            }
        }

        /// <summary>
        /// Selects the actions within the specified route context.
        /// </summary>
        /// <param name="routeContext">The current <see cref="RouteContext">context</see>.</param>
        /// <returns>A <see cref="IEnumerable{T}">sequence</see> of matching <see cref="ControllerActionDescriptor">actions</see>.</returns>
        public IEnumerable<ControllerActionDescriptor> SelectAction( RouteContext routeContext )
        {
            if ( routeContext == null )
            {
                throw new ArgumentNullException( nameof( routeContext ) );
            }

            var httpContext = routeContext.HttpContext;
            var services = httpContext.RequestServices;
            var actionCollectionProvider = services.GetRequiredService<IActionDescriptorCollectionProvider>();
            var actionDescriptors = actionCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>().ToArray();
            var comparer = StringComparer.OrdinalIgnoreCase;

            foreach ( var controllerResult in SelectController( routeContext ) )
            {
                var controllerName = controllerResult.ControllerName;
                var actionName = controllerResult.Values[ODataRouteConstants.Action].ToString();

                foreach ( var action in actionDescriptors )
                {
                    if ( action.ControllerName == controllerName && comparer.Equals( actionName, action.ActionName ) )
                    {
                        yield return action;
                    }
                }
            }
        }

        IDictionary<ODataPathTemplate, ControllerActionDescriptor> BuildAttributeMappings( IEnumerable<ControllerActionDescriptor> actions )
        {
            var attributeMappings = new Dictionary<ODataPathTemplate, ControllerActionDescriptor>();

            foreach ( var action in actions )
            {
                if ( !action.ControllerTypeInfo.IsODataController() || !ShouldMapAction( action ) )
                {
                    continue;
                }

                foreach ( var prefix in GetODataRoutePrefixes( action ) )
                {
                    var pathTemplates = GetODataPathTemplates( prefix, action );

                    foreach ( var pathTemplate in pathTemplates )
                    {
                        attributeMappings.Add( pathTemplate, action );
                    }
                }
            }

            return attributeMappings;
        }

        static IEnumerable<string> GetODataRoutePrefixes( ControllerActionDescriptor controllerAction )
        {
            var prefixAttributes = controllerAction.ControllerTypeInfo.GetCustomAttributes<ODataRoutePrefixAttribute>( inherit: false );
            return GetODataRoutePrefixes( prefixAttributes, controllerAction.ControllerTypeInfo?.FullName ?? string.Empty );
        }

        IEnumerable<ODataPathTemplate> GetODataPathTemplates( string prefix, ControllerActionDescriptor controllerAction )
        {
            var routeAttributes = controllerAction.MethodInfo.GetCustomAttributes<ODataRouteAttribute>( inherit: false );
            var perRouteContainer = serviceProvider.GetRequiredService<IPerRouteContainer>();
            var requestContainer = perRouteContainer.GetODataRootContainer( routeName );
            var controllerName = controllerAction.ControllerName;
            var actionName = controllerAction.ActionName;

            return routeAttributes.Select( route => GetODataPathTemplate( prefix, route.PathTemplate, requestContainer, controllerName, actionName ) ).Where( template => template != null );
        }
    }
}