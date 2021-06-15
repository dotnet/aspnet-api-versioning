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
    using Microsoft.Extensions.Options;
    using Microsoft.OData.Edm;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider">HTTP configuration</see>.</param>
        public VersionedAttributeRoutingConvention( string routeName, IServiceProvider serviceProvider )
        {
            if ( serviceProvider == null )
            {
                throw new ArgumentNullException( nameof( serviceProvider ) );
            }

            var perRouteContainer = serviceProvider.GetRequiredService<IPerRouteContainer>();
            var rootContainer = perRouteContainer.GetODataRootContainer( routeName );

            RouteName = routeName;
            ODataPathTemplateHandler = rootContainer.GetRequiredService<IODataPathTemplateHandler>();
        }

        /// <summary>
        /// Returns a value indicating whether the specified action should be mapped using attribute routing conventions.
        /// </summary>
        /// <param name="action">The <see cref="ControllerActionDescriptor">controller action descriptor</see> to evaluate.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to evaluate.</param>
        /// <returns>True if the <paramref name="action"/> should be mapped as an OData action or function; otherwise, false.</returns>
        /// <remarks>This method will match any OData action that explicitly or implicitly matches the API version applied
        /// to the associated <see cref="ApiVersionModel">model</see>.</remarks>
        public virtual bool ShouldMapAction( ControllerActionDescriptor action, ApiVersion? apiVersion ) => action.IsMappedTo( apiVersion );

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

            var version = SelectApiVersion( routeContext );
            var attributeMappings = attributeMappingsPerApiVersion.GetOrAdd( version, key => BuildAttributeMappings( key, routeContext ) );
            var values = new Dictionary<string, object>();
            var feature = routeContext.HttpContext.ODataFeature();
            var odataPath = feature.Path;
            var routeData = routeContext.RouteData.Values;

            foreach ( var attributeMapping in attributeMappings )
            {
                var template = attributeMapping.Key;
                var action = attributeMapping.Value;

                if ( !template.TryMatch( odataPath, values ) )
                {
                    continue;
                }

                foreach ( var value in values )
                {
                    if ( IsODataRouteParameter( value ) )
                    {
                        feature.RoutingConventionsStore[value.Key] = value.Value;
                    }
                    else
                    {
                        routeData[value.Key] = value.Value;
                    }
                }

                values[ODataRouteConstants.Action] = action.ActionName;

                yield return new SelectControllerResult( action.ControllerName, values );
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

        /// <summary>
        /// Selects the API version from the given HTTP request.
        /// </summary>
        /// <param name="routeContext">The current <see cref="RouteContext">context</see>.</param>
        /// <returns>The selected <see cref="ApiVersion">API version</see>.</returns>
        protected virtual ApiVersion SelectApiVersion( RouteContext routeContext )
        {
            if ( routeContext == null )
            {
                throw new ArgumentNullException( nameof( routeContext ) );
            }

            var httpContext = routeContext.HttpContext;
            var feature = httpContext.ApiVersioningFeature();
            ApiVersion? version;

            try
            {
                version = feature.RequestedApiVersion;
            }
            catch ( AmbiguousApiVersionException )
            {
                version = default;
            }

            if ( version != null )
            {
                return version;
            }

            var options = httpContext.RequestServices.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;

            if ( !options.AssumeDefaultVersionWhenUnspecified )
            {
                return ApiVersion.Neutral;
            }

            var modelSelector = httpContext.Request.GetRequestContainer().GetRequiredService<IEdmModelSelector>();
            var versionSelector = options.ApiVersionSelector;
            var model = new ApiVersionModel( modelSelector.ApiVersions, Enumerable.Empty<ApiVersion>() );

            return versionSelector.SelectVersion( httpContext.Request, model );
        }

        static IEnumerable<string> GetODataRoutePrefixes( ControllerActionDescriptor controllerAction )
        {
            var prefixAttributes = controllerAction.ControllerTypeInfo.GetCustomAttributes<ODataRoutePrefixAttribute>( inherit: false );
            return GetODataRoutePrefixes( prefixAttributes, controllerAction.ControllerTypeInfo.FullName! );
        }

        IReadOnlyDictionary<ODataPathTemplate, ControllerActionDescriptor> BuildAttributeMappings( ApiVersion version, RouteContext routeContext )
        {
            var httpContext = routeContext.HttpContext;
            var services = httpContext.RequestServices;
            var provider = services.GetRequiredService<IActionDescriptorCollectionProvider>();
            var actions = provider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>();
            var attributeMappings = new Dictionary<ODataPathTemplate, ControllerActionDescriptor>();
            var serviceProvider = httpContext.Request.GetRequestContainer();

            foreach ( var action in actions )
            {
                if ( !action.ControllerTypeInfo.IsODataController() || !ShouldMapAction( action, version ) )
                {
                    continue;
                }

                if ( action.AttributeRouteInfo is ODataAttributeRouteInfo routeInfo && routeInfo.ODataTemplate != null )
                {
                    attributeMappings.Add( routeInfo.ODataTemplate, action );
                    continue;
                }

                foreach ( var prefix in GetODataRoutePrefixes( action ) )
                {
                    var pathTemplates = GetODataPathTemplates( prefix, action, serviceProvider );

                    foreach ( var pathTemplate in pathTemplates )
                    {
                        attributeMappings.Add( pathTemplate, action );
                    }
                }
            }

            return attributeMappings;
        }

        IEnumerable<ODataPathTemplate> GetODataPathTemplates( string prefix, ControllerActionDescriptor controllerAction, IServiceProvider serviceProvider )
        {
            var routeAttributes = controllerAction.MethodInfo.GetCustomAttributes<ODataRouteAttribute>( inherit: false );

            foreach ( var route in routeAttributes )
            {
                var template = GetODataPathTemplate( prefix, route.PathTemplate, serviceProvider );

                if ( template != null )
                {
                    yield return template;
                }
            }
        }
    }
}