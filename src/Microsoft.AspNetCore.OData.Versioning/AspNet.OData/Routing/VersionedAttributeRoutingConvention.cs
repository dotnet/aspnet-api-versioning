﻿namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNet.OData.Routing.Template;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
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
            : this( routeName, serviceProvider, default, apiVersion ) { }

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
                if ( model.IsApiVersionNeutral || model.DeclaredApiVersions.Contains( ApiVersion ) )
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
        /// <param name="routeContext">The current <see cref="RouteContext">context</see>.</param>
        /// <returns>The name of the selected controller; otherwise, <c>null</c> if the request isn't handled by this convention.</returns>
        protected virtual IEnumerable<SelectControllerResult> SelectController( RouteContext routeContext )
        {
            Arg.NotNull( routeContext, nameof( routeContext ) );

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
            Arg.NotNull( routeContext, nameof( routeContext ) );

            var services = routeContext.HttpContext.RequestServices;
            var actionCollectionProvider = services.GetRequiredService<IActionDescriptorCollectionProvider>();
            var actionDescriptors = actionCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>().ToArray();

            foreach ( var controllerResult in SelectController( routeContext ) )
            {
                var controllerName = controllerResult.ControllerName;
                var attributeRouteData = controllerResult.Values;

                foreach ( var action in actionDescriptors )
                {
                    if ( action.ControllerName == controllerName )
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
    }
}