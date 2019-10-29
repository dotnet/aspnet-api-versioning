namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNet.OData.Routing.Template;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    public partial class VersionedAttributeRoutingConvention : IODataRoutingConvention
    {
        const string AttributeRouteData = nameof( AttributeRouteData );
        static readonly DefaultODataPathHandler defaultPathHandler = new DefaultODataPathHandler();

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the convention.</param>
        public VersionedAttributeRoutingConvention( string routeName, HttpConfiguration configuration, ApiVersion apiVersion )
            : this( routeName, configuration, defaultPathHandler, apiVersion ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="pathTemplateHandler">The <see cref="IODataPathTemplateHandler">OData path template handler</see> associated with the routing convention.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the convention.</param>
        public VersionedAttributeRoutingConvention( string routeName, HttpConfiguration configuration, IODataPathTemplateHandler pathTemplateHandler, ApiVersion apiVersion )
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            this.routeName = routeName;
            ODataPathTemplateHandler = pathTemplateHandler;
            ApiVersion = apiVersion;

            if ( pathTemplateHandler is IODataPathHandler pathHandler && pathHandler.UrlKeyDelimiter == null )
            {
                var urlKeyDelimiter = configuration.GetUrlKeyDelimiter();
                pathHandler.UrlKeyDelimiter = urlKeyDelimiter;
            }

            var initialized = false;
            var initializer = configuration.Initializer;

            configuration.Initializer = config =>
            {
                if ( initialized )
                {
                    return;
                }

                initialized = true;
                initializer?.Invoke( config );

                var controllerSelector = configuration.Services.GetHttpControllerSelector();
                attributeMappings = BuildAttributeMappings( controllerSelector.GetControllerMapping().Values );
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="controllers">The <see cref="IEnumerable{T}">sequence</see> of <see cref="HttpControllerDescriptor">controller descriptors</see>.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the convention.</param>
        public VersionedAttributeRoutingConvention( string routeName, IEnumerable<HttpControllerDescriptor> controllers, ApiVersion apiVersion )
            : this( routeName, controllers, defaultPathHandler, apiVersion ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="controllers">The <see cref="IEnumerable{T}">sequence</see> of <see cref="HttpControllerDescriptor">controller descriptors</see>
        /// associated with the routing convention.</param>
        /// <param name="pathTemplateHandler">The <see cref="IODataPathTemplateHandler">OData path template handler</see> associated with the routing convention.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the convention.</param>
        public VersionedAttributeRoutingConvention( string routeName, IEnumerable<HttpControllerDescriptor> controllers, IODataPathTemplateHandler pathTemplateHandler, ApiVersion apiVersion )
        {
            if ( controllers == null )
            {
                throw new ArgumentNullException( nameof( controllers ) );
            }

            this.routeName = routeName;
            ODataPathTemplateHandler = pathTemplateHandler;
            ApiVersion = apiVersion;
            attributeMappings = BuildAttributeMappings( controllers );
        }

        IDictionary<ODataPathTemplate, HttpActionDescriptor> AttributeMappings => attributeMappings ?? throw new InvalidOperationException( SR.ObjectNotYetInitialized );

        /// <summary>
        /// Returns a value indicating whether the specified controller should be mapped using attribute routing conventions.
        /// </summary>
        /// <param name="controller">The <see cref="HttpControllerDescriptor">controller descriptor</see> to evaluate.</param>
        /// <returns>True if the <paramref name="controller"/> should be mapped as an OData controller; otherwise, false.</returns>
        /// <remarks>The default implementation always returns <c>true</c>.</remarks>
        public virtual bool ShouldMapController( HttpControllerDescriptor controller ) => true;

        /// <summary>
        /// Returns a value indicating whether the specified action should be mapped using attribute routing conventions.
        /// </summary>
        /// <param name="action">The <see cref="HttpActionDescriptor">action descriptor</see> to evaluate.</param>
        /// <returns>True if the <paramref name="action"/> should be mapped as an OData action or function; otherwise, false.</returns>
        /// <remarks>This method will match any OData action that explicitly or implicitly matches the API version applied
        /// to the associated <see cref="ApiVersionModel">model</see>.</remarks>
        public virtual bool ShouldMapAction( HttpActionDescriptor action ) => action.IsMappedTo( ApiVersion );

        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="request">The request.</param>
        /// <returns><c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected controller.</returns>
        public virtual string? SelectController( ODataPath odataPath, HttpRequestMessage request )
        {
            if ( request == null )
            {
                throw new ArgumentNullException( nameof( request ) );
            }

            var values = new Dictionary<string, object>();

            foreach ( var attributeMapping in AttributeMappings )
            {
                var template = attributeMapping.Key;
                var action = attributeMapping.Value;

                if ( action.SupportedHttpMethods.Contains( request.Method ) && template.TryMatch( odataPath, values ) )
                {
                    values[ODataRouteConstants.Action] = action.ActionName;
                    request.Properties[AttributeRouteData] = values;

                    return action.ControllerDescriptor.ControllerName;
                }
            }

            return null;
        }

        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMap">The action map.</param>
        /// <returns><c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected action.</returns>
        public virtual string? SelectAction( ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap )
        {
            if ( controllerContext == null )
            {
                throw new ArgumentNullException( nameof( controllerContext ) );
            }

            var request = controllerContext.Request;
            var properties = request.Properties;

            if ( !properties.TryGetValue( AttributeRouteData, out var value ) || !( value is IDictionary<string, object> attributeRouteData ) )
            {
                return null;
            }

            var routeData = request.GetRouteData();
            var routingConventionsStore = request.ODataProperties().RoutingConventionsStore;

            foreach ( var item in attributeRouteData )
            {
                if ( IsODataRouteParameter( item ) )
                {
                    routingConventionsStore.Add( item );
                }
                else
                {
                    routeData.Values.Add( item );
                }
            }

            return attributeRouteData[ODataRouteConstants.Action]?.ToString();
        }

        IDictionary<ODataPathTemplate, HttpActionDescriptor> BuildAttributeMappings( IEnumerable<HttpControllerDescriptor> controllers )
        {
            var attributeMappings = new Dictionary<ODataPathTemplate, HttpActionDescriptor>();

            foreach ( var controller in controllers )
            {
                if ( !controller.ControllerType.IsODataController() || !ShouldMapController( controller ) )
                {
                    continue;
                }

                var actionSelector = controller.Configuration.Services.GetActionSelector();
                var actionMapping = actionSelector.GetActionMapping( controller );
                var actions = actionMapping.SelectMany( a => a ).ToArray();

                foreach ( var prefix in GetODataRoutePrefixes( controller ) )
                {
                    foreach ( var action in actions )
                    {
                        if ( !ShouldMapAction( action ) )
                        {
                            continue;
                        }

                        var pathTemplates = GetODataPathTemplates( prefix, action );

                        foreach ( var pathTemplate in pathTemplates )
                        {
                            attributeMappings.Add( pathTemplate, action );
                        }
                    }
                }
            }

            return attributeMappings;
        }

        static IEnumerable<string> GetODataRoutePrefixes( HttpControllerDescriptor controllerDescriptor )
        {
            var prefixAttributes = controllerDescriptor.GetCustomAttributes<ODataRoutePrefixAttribute>( inherit: false );
            return GetODataRoutePrefixes( prefixAttributes, controllerDescriptor.ControllerType.FullName );
        }

        IEnumerable<ODataPathTemplate> GetODataPathTemplates( string prefix, HttpActionDescriptor action )
        {
            var routeAttributes = action.GetCustomAttributes<ODataRouteAttribute>( inherit: false );
            var serviceProvider = action.Configuration.GetODataRootContainer( routeName );
            var controllerName = action.ControllerDescriptor.ControllerName;
            var actionName = action.ActionName;

            return routeAttributes.Select( route => GetODataPathTemplate( prefix, route.PathTemplate, serviceProvider, controllerName, actionName ) ).Where( template => template != null );
        }
    }
}