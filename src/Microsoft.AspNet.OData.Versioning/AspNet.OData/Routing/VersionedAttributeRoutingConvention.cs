namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNet.OData.Routing.Template;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        public VersionedAttributeRoutingConvention( string routeName, HttpConfiguration configuration )
            : this( routeName, configuration, new DefaultODataPathHandler() ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="configuration">The current <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="pathTemplateHandler">The <see cref="IODataPathTemplateHandler">OData path template handler</see> associated with the routing convention.</param>
        public VersionedAttributeRoutingConvention( string routeName, HttpConfiguration configuration, IODataPathTemplateHandler pathTemplateHandler )
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            RouteName = routeName;
            ODataPathTemplateHandler = pathTemplateHandler;

            if ( pathTemplateHandler is IODataPathHandler pathHandler && pathHandler.UrlKeyDelimiter == null )
            {
                var urlKeyDelimiter = configuration.GetUrlKeyDelimiter();
                pathHandler.UrlKeyDelimiter = urlKeyDelimiter;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified controller should be mapped using attribute routing conventions.
        /// </summary>
        /// <param name="controller">The <see cref="HttpControllerDescriptor">controller descriptor</see> to evaluate.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to evaluate.</param>
        /// <returns>True if the <paramref name="controller"/> should be mapped as an OData controller; otherwise, false.</returns>
        /// <remarks>The default implementation always returns <c>true</c>.</remarks>
        public virtual bool ShouldMapController( HttpControllerDescriptor controller, ApiVersion? apiVersion )
        {
            var model = controller.GetApiVersionModel();
            return model.IsApiVersionNeutral || model.DeclaredApiVersions.Contains( apiVersion );
        }

        /// <summary>
        /// Returns a value indicating whether the specified action should be mapped using attribute routing conventions.
        /// </summary>
        /// <param name="action">The <see cref="HttpActionDescriptor">action descriptor</see> to evaluate.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to evaluate.</param>
        /// <returns>True if the <paramref name="action"/> should be mapped as an OData action or function; otherwise, false.</returns>
        /// <remarks>This method will match any OData action that explicitly or implicitly matches the API version applied
        /// to the associated <see cref="ApiVersionModel">model</see>.</remarks>
        public virtual bool ShouldMapAction( HttpActionDescriptor action, ApiVersion? apiVersion ) => action.IsMappedTo( apiVersion );

        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="request">The request.</param>
        /// <returns><c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected controller.</returns>
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

            if ( odataPath.Segments.Count == 0 )
            {
                return null;
            }

            var version = SelectApiVersion( request );
            var attributeMappings = attributeMappingsPerApiVersion.GetOrAdd( version, key => BuildAttributeMappings( key, request ) );
            var values = new Dictionary<string, object>();

            foreach ( var attributeMapping in attributeMappings )
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

        /// <summary>
        /// Selects the API version from the given HTTP request.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage">HTTP request</see>.</param>
        /// <returns>The selected <see cref="ApiVersion">API version</see>.</returns>
        protected virtual ApiVersion SelectApiVersion( HttpRequestMessage request )
        {
            var version = request.GetRequestedApiVersionOrReturnBadRequest();

            if ( version != null )
            {
                return version;
            }

            var options = request.GetApiVersioningOptions();

            if ( !options.AssumeDefaultVersionWhenUnspecified )
            {
                return version ?? ApiVersion.Neutral;
            }

            var modelSelector = request.GetRequestContainer().GetRequiredService<IEdmModelSelector>();
            var versionSelector = request.GetApiVersioningOptions().ApiVersionSelector;
            var model = new ApiVersionModel( modelSelector.ApiVersions, Enumerable.Empty<ApiVersion>() );

            return versionSelector.SelectVersion( request, model );
        }

        static IEnumerable<string> GetODataRoutePrefixes( HttpControllerDescriptor controllerDescriptor )
        {
            var prefixAttributes = controllerDescriptor.GetCustomAttributes<ODataRoutePrefixAttribute>( inherit: false );
            return GetODataRoutePrefixes( prefixAttributes, controllerDescriptor.ControllerType.FullName );
        }

        IReadOnlyDictionary<ODataPathTemplate, HttpActionDescriptor> BuildAttributeMappings( ApiVersion version, HttpRequestMessage request )
        {
            var configuration = request.GetConfiguration();
            var services = configuration.Services;
            var controllerSelector = services.GetHttpControllerSelector();
            var controllers = controllerSelector.GetControllerMapping().Values.ToArray();
            var attributeMappings = new Dictionary<ODataPathTemplate, HttpActionDescriptor>();
            var actionSelector = services.GetActionSelector();
            var serviceProvider = request.GetRequestContainer();

            for ( var i = 0; i < controllers.Length; i++ )
            {
                foreach ( var controller in controllers[i].AsEnumerable() )
                {
                    if ( !controller.ControllerType.IsODataController() || !ShouldMapController( controller, version ) )
                    {
                        continue;
                    }

                    var actionMapping = actionSelector.GetActionMapping( controller );
                    var actions = actionMapping.SelectMany( a => a ).ToArray();

                    foreach ( var prefix in GetODataRoutePrefixes( controller ) )
                    {
                        foreach ( var action in actions )
                        {
                            if ( !ShouldMapAction( action, version ) )
                            {
                                continue;
                            }

                            var pathTemplates = GetODataPathTemplates( prefix, action, serviceProvider );

                            foreach ( var pathTemplate in pathTemplates )
                            {
                                attributeMappings.Add( pathTemplate, action );
                            }
                        }
                    }
                }
            }

            return attributeMappings;
        }

        IEnumerable<ODataPathTemplate> GetODataPathTemplates( string prefix, HttpActionDescriptor action, IServiceProvider serviceProvider )
        {
            var routeAttributes = action.GetCustomAttributes<ODataRouteAttribute>( inherit: false );

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