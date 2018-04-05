namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNet.OData.Routing.Template;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using static System.StringComparison;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class VersionedAttributeRoutingConvention : IODataRoutingConvention
    {
        static readonly string[] SupportedHttpMethodConventions = new[]
        {
            "GET",
            "PUT",
            "POST",
            "DELETE",
            "PATCH",
            "HEAD",
            "OPTIONS",
        };
        readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider">HTTP configuration</see>.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the convention.</param>
        public VersionedAttributeRoutingConvention( string routeName, IServiceProvider serviceProvider, ApiVersion apiVersion )
            : this( routeName, serviceProvider, default( IODataPathTemplateHandler ), apiVersion ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedAttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="serviceProvider">The current <see cref="IServiceProvider">HTTP configuration</see>.</param>
        /// <param name="pathTemplateHandler">The <see cref="IODataPathTemplateHandler">OData path template handler</see> associated with the routing convention.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the convention.</param>
        public VersionedAttributeRoutingConvention( string routeName, IServiceProvider serviceProvider, IODataPathTemplateHandler pathTemplateHandler, ApiVersion apiVersion )
        {
            Arg.NotNull( routeName, nameof( routeName ) );
            Arg.NotNull( serviceProvider, nameof( serviceProvider ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );

            this.routeName = routeName;
            this.serviceProvider = serviceProvider;
            ApiVersion = apiVersion;

            if ( ( ODataPathTemplateHandler = pathTemplateHandler ) == null )
            {
                var perRouteContainer = serviceProvider.GetRequiredService<IPerRouteContainer>();
                var rootContainer = perRouteContainer.GetODataRootContainer( routeName );
                ODataPathTemplateHandler = rootContainer.GetRequiredService<IODataPathTemplateHandler>();
            }
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
        /// <remarks>This method will match any OData action that explicitly or implicitly matches matches the API version applied
        /// to the associated <see cref="ApiVersionModel">model</see>.</remarks>
        public virtual bool ShouldMapAction( ControllerActionDescriptor action )
        {
            Arg.NotNull( action, nameof( action ) );

            var model = action.GetProperty<ApiVersionModel>();

            if ( model != null )
            {
                if ( model.DeclaredApiVersions.Contains( ApiVersion ) )
                {
                    return true;
                }

                var implicitlyUnmatchable = model.DeclaredApiVersions.Count > 0;

                if ( implicitlyUnmatchable )
                {
                    return false;
                }
            }

            model = action.GetProperty<ControllerModel>()?.GetProperty<ApiVersionModel>();

            if ( model == null )
            {
                return false;
            }

            return model.IsApiVersionNeutral || model.DeclaredApiVersions.Contains( ApiVersion );
        }

        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="request">The request.</param>
        /// <returns>The name of the selected controller; otherwise, <c>null</c> if the request isn't handled by this convention.</returns>
        protected virtual SelectControllerResult SelectController( ODataPath odataPath, HttpRequest request )
        {
            Arg.NotNull( odataPath, nameof( odataPath ) );
            Arg.NotNull( request, nameof( request ) );

            var values = new Dictionary<string, object>();

            foreach ( var attributeMapping in AttributeMappings )
            {
                var template = attributeMapping.Key;
                var action = attributeMapping.Value;
                var supportedHttpMethods = GetSupportedHttpMethods( action );

                if ( supportedHttpMethods.Contains( request.Method ) && template.TryMatch( odataPath, values ) )
                {
                    values["action"] = action.ActionName;
                    return new SelectControllerResult( action.ControllerName, values );
                }
            }

            return default( SelectControllerResult );
        }

        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="routeContext">The current <see cref="RouteContext">context</see>.</param>
        /// <param name="controllerResult">The current <see cref="SelectControllerResult">controller selection result</see>.</param>
        /// <returns>The name of the selected action; otherwise, <c>null</c> if the request isn't handled by this convention.</returns>
        protected virtual string SelectAction( RouteContext routeContext, SelectControllerResult controllerResult )
        {
            Arg.NotNull( routeContext, nameof( routeContext ) );
            Arg.NotNull( controllerResult, nameof( controllerResult ) );

            var attributeRouteData = controllerResult.Values;
            var feature = routeContext.HttpContext.ODataFeature();
            IDictionary<string, object> routeData = routeContext.RouteData.Values;

            foreach ( var item in attributeRouteData )
            {
                if ( IsODataRouteParameter( item ) )
                {
                    feature.RoutingConventionsStore.Add( item );
                }
                else
                {
                    routeData.Add( item );
                }
            }

            return attributeRouteData["action"] as string;
        }

        /// <summary>
        /// Selects the actions within the specified route context.
        /// </summary>
        /// <param name="routeContext">The current <see cref="RouteContext">context</see>.</param>
        /// <returns>A <see cref="IEnumerable{T}">sequence</see> of matching <see cref="ControllerActionDescriptor">actions</see>.</returns>
        public IEnumerable<ControllerActionDescriptor> SelectAction( RouteContext routeContext )
        {
            Arg.NotNull( routeContext, nameof( routeContext ) );

            var httpContext = routeContext.HttpContext;
            var actionCollectionProvider = httpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
            var odataPath = httpContext.ODataFeature().Path;
            var controllerResult = SelectController( odataPath, httpContext.Request );

            if ( controllerResult == null )
            {
                return null;
            }

            var actionName = SelectAction( routeContext, controllerResult );

            if ( string.IsNullOrEmpty( actionName ) )
            {
                return null;
            }

            var actionDescriptors = actionCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>().Where( c => c.ControllerName == controllerResult.ControllerName );

            return actionDescriptors.Where( c => string.Equals( c.ActionName, actionName, OrdinalIgnoreCase ) );
        }

        IDictionary<ODataPathTemplate, ControllerActionDescriptor> BuildAttributeMappings( IEnumerable<ControllerActionDescriptor> actions )
        {
            var attributeMappings = new Dictionary<ODataPathTemplate, ControllerActionDescriptor>();

            foreach ( var action in actions )
            {
                if ( !IsODataController( action ) || !ShouldMapAction( action ) )
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

        static bool IsODataController( ControllerActionDescriptor controller ) => typeof( ODataController ).GetTypeInfo().IsAssignableFrom( controller.ControllerTypeInfo );

        static IEnumerable<string> GetODataRoutePrefixes( ControllerActionDescriptor controllerAction )
        {
            Contract.Requires( controllerAction != null );
            var prefixAttributes = controllerAction.ControllerTypeInfo.GetCustomAttributes<ODataRoutePrefixAttribute>( inherit: false );
            return GetODataRoutePrefixes( prefixAttributes, controllerAction.ControllerTypeInfo.FullName );
        }

        IEnumerable<ODataPathTemplate> GetODataPathTemplates( string prefix, ControllerActionDescriptor controllerAction )
        {
            Contract.Requires( controllerAction != null );

            var routeAttributes = controllerAction.MethodInfo.GetCustomAttributes<ODataRouteAttribute>( inherit: false );
            var perRouteContainer = serviceProvider.GetRequiredService<IPerRouteContainer>();
            var requestContainer = perRouteContainer.GetODataRootContainer( routeName );
            var controllerName = controllerAction.ControllerName;
            var actionName = controllerAction.ActionName;

            return routeAttributes.Select( route => GetODataPathTemplate( prefix, route.PathTemplate, requestContainer, controllerName, actionName ) ).Where( template => template != null );
        }

        static IReadOnlyCollection<string> GetSupportedHttpMethods( ControllerActionDescriptor action )
        {
            Contract.Requires( action != null );
            Contract.Ensures( Contract.Result<IReadOnlyCollection<string>>() != null );

            var actionConstraints = ( action.ActionConstraints ?? Array.Empty<IActionConstraintMetadata>() ).OfType<HttpMethodActionConstraint>();
            var attributedHttpMethods = actionConstraints.SelectMany( ac => ac.HttpMethods );
            var httpMethods = new HashSet<string>( attributedHttpMethods, StringComparer.OrdinalIgnoreCase );

            if ( httpMethods.Count > 0 )
            {
                return httpMethods;
            }

            foreach ( var supportedHttpMethod in SupportedHttpMethodConventions )
            {
                if ( action.MethodInfo.Name.StartsWith( supportedHttpMethod, OrdinalIgnoreCase ) )
                {
                    httpMethods.Add( supportedHttpMethod );
                }
            }

            if ( httpMethods.Count == 0 )
            {
                httpMethods.Add( "POST" );
            }

            return httpMethods;
        }
    }
}