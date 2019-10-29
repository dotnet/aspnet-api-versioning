namespace System.Web.Http
{
    using Microsoft;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Routing;
    using Microsoft.Web.Http.Versioning;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Routing;
    using static Microsoft.OData.ServiceLifetime;
    using static System.String;
    using static System.StringComparison;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        const string ContainerBuilderFactoryKey = "Microsoft.AspNet.OData.ContainerBuilderFactoryKey";
        const string RootContainerMappingsKey = "Microsoft.AspNet.OData.RootContainerMappingsKey";
        const string UrlKeyDelimiterKey = "Microsoft.AspNet.OData.UrlKeyDelimiterKey";
        const string UnversionedRouteSuffix = "-Unversioned";

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            Action<IContainerBuilder>? configureAction ) =>
            MapVersionedODataRoutes( configuration, routeName, routePrefix, models, configureAction, default, default );

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler">OData batch handler</see>.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            Action<IContainerBuilder>? configureAction,
            ODataBatchHandler? batchHandler ) =>
            MapVersionedODataRoutes( configuration, routeName, routePrefix, models, configureAction, default, batchHandler );

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <param name="configureRoutingConventions">The configuring action to add or update routing conventions.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            Action<IContainerBuilder>? configureAction,
            Action<ODataConventionConfigurationContext>? configureRoutingConventions ) =>
            MapVersionedODataRoutes( configuration, routeName, routePrefix, models, configureAction, configureRoutingConventions, default );

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <param name="configureRoutingConventions">The configuring action to add or update routing conventions.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler">OData batch handler</see>.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            Action<IContainerBuilder>? configureAction,
            Action<ODataConventionConfigurationContext>? configureRoutingConventions,
            ODataBatchHandler? batchHandler )
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            if ( models == null )
            {
                throw new ArgumentNullException( nameof( models ) );
            }

            object ConfigureRoutingConventions( IEdmModel model, string versionedRouteName, ApiVersion apiVersion )
            {
                var routingConventions = VersionedODataRoutingConventions.CreateDefault();
                var context = new ODataConventionConfigurationContext( configuration, versionedRouteName, model, apiVersion, routingConventions );

                model.SetAnnotationValue( model, new ApiVersionAnnotation( apiVersion ) );
                routingConventions.Insert( 0, new VersionedAttributeRoutingConvention( versionedRouteName, configuration, apiVersion ) );
                configureRoutingConventions?.Invoke( context );

                return context.RoutingConventions.ToArray();
            }

            if ( !IsNullOrEmpty( routePrefix ) )
            {
                routePrefix = routePrefix.TrimEnd( '/' );
            }

            var routes = configuration.Routes;
            var unversionedRouteName = routeName + UnversionedRouteSuffix;

            if ( batchHandler != null )
            {
                batchHandler.ODataRouteName = unversionedRouteName;
                var batchTemplate = IsNullOrEmpty( routePrefix ) ? ODataRouteConstants.Batch : routePrefix + '/' + ODataRouteConstants.Batch;
                routes.MapHttpBatchRoute( routeName + nameof( ODataRouteConstants.Batch ), batchTemplate, batchHandler );
            }

            var odataRoutes = new List<ODataRoute>();
            var unversionedConstraints = new List<IHttpRouteConstraint>();

            foreach ( var model in models )
            {
                var versionedRouteName = routeName;
                var annotation = model.GetAnnotationValue<ApiVersionAnnotation>( model ) ?? throw new InvalidOperationException( LocalSR.MissingAnnotation.FormatDefault( typeof( ApiVersionAnnotation ).Name ) );
                var apiVersion = annotation.ApiVersion;
                var routeConstraint = MakeVersionedODataRouteConstraint( apiVersion, ref versionedRouteName );

                unversionedConstraints.Add( new ODataPathRouteConstraint( versionedRouteName ) );

                var rootContainer = configuration.CreateODataRootContainer(
                                        versionedRouteName,
                                        builder =>
                                        {
                                            builder.AddService( Singleton, typeof( IEdmModel ), sp => model )
                                                   .AddService( Singleton, typeof( IEnumerable<IODataRoutingConvention> ), sp => ConfigureRoutingConventions( model, versionedRouteName, apiVersion ) );
                                            configureAction?.Invoke( builder );
                                        } );

                var pathHandler = rootContainer.GetRequiredService<IODataPathHandler>();

                if ( pathHandler != null && pathHandler.UrlKeyDelimiter == null )
                {
                    pathHandler.UrlKeyDelimiter = configuration.GetUrlKeyDelimiter();
                }

                rootContainer.InitializeAttributeRouting();

                var route = default( ODataRoute );
                var messageHandler = rootContainer.GetService<HttpMessageHandler>();
                var options = configuration.GetApiVersioningOptions();

                if ( messageHandler == null )
                {
                    route = new ODataRoute( routePrefix, routeConstraint );
                }
                else
                {
                    route = new ODataRoute( routePrefix, routeConstraint, defaults: null, constraints: null, dataTokens: null, handler: messageHandler );
                }

                routes.Add( versionedRouteName, route );
                AddApiVersionConstraintIfNecessary( route, options );
                odataRoutes.Add( route );
            }

            configuration.AddRouteToRespondWithBadRequestWhenAtLeastOneRouteCouldMatch( unversionedRouteName, routePrefix, odataRoutes, unversionedConstraints, configureAction );

            return odataRoutes;
        }

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes( this HttpConfiguration configuration, string routeName, string routePrefix, IEnumerable<IEdmModel> models ) =>
            MapVersionedODataRoutes( configuration, routeName, routePrefix, models, new DefaultODataPathHandler(), VersionedODataRoutingConventions.CreateDefault(), default );

        /// <summary>
        /// Maps the specified versioned OData routes. When the <paramref name="batchHandler"/> is provided, it will create a
        /// '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler">OData batch handler</see>.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            ODataBatchHandler? batchHandler ) =>
            MapVersionedODataRoutes( configuration, routeName, routePrefix, models, new DefaultODataPathHandler(), VersionedODataRoutingConventions.CreateDefault(), batchHandler );

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler">OData path handler</see> to use for parsing the OData path.</param>
        /// <param name="routingConventions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IODataRoutingConvention">OData routing conventions</see>
        /// to use for controller and action selection.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions ) =>
            MapVersionedODataRoutes( configuration, routeName, routePrefix, models, pathHandler, routingConventions, default );

        /// <summary>
        /// Maps the specified versioned OData routes. When the <paramref name="batchHandler"/> is provided, it will create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler">OData path handler</see> to use for parsing the OData path.</param>
        /// <param name="routingConventions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IODataRoutingConvention">OData routing conventions</see>
        /// to use for controller and action selection.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler">OData batch handler</see>.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions,
            ODataBatchHandler? batchHandler )
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            if ( models == null )
            {
                throw new ArgumentNullException( nameof( models ) );
            }

            var routeConventions = VersionedODataRoutingConventions.AddOrUpdate( routingConventions.ToList() );
            var routes = configuration.Routes;
            var unversionedRouteName = routeName + UnversionedRouteSuffix;

            if ( !IsNullOrEmpty( routePrefix ) )
            {
                routePrefix = routePrefix.TrimEnd( '/' );
            }

            if ( batchHandler != null )
            {
                batchHandler.ODataRouteName = unversionedRouteName;
                var batchTemplate = IsNullOrEmpty( routePrefix ) ? ODataRouteConstants.Batch : routePrefix + '/' + ODataRouteConstants.Batch;
                routes.MapHttpBatchRoute( routeName + nameof( ODataRouteConstants.Batch ), batchTemplate, batchHandler );
            }

            if ( pathHandler != null && pathHandler.UrlKeyDelimiter == null )
            {
                pathHandler.UrlKeyDelimiter = configuration.GetUrlKeyDelimiter();
            }

            routeConventions.Insert( 0, default! );

            var odataRoutes = new List<ODataRoute>();
            var unversionedConstraints = new List<IHttpRouteConstraint>();

            foreach ( var model in models )
            {
                var versionedRouteName = routeName;
                var annotation = model.GetAnnotationValue<ApiVersionAnnotation>( model ) ?? throw new InvalidOperationException( LocalSR.MissingAnnotation.FormatDefault( typeof( ApiVersionAnnotation ).Name ) );
                var apiVersion = annotation.ApiVersion;
                var routeConstraint = MakeVersionedODataRouteConstraint( apiVersion, ref versionedRouteName );

                routeConventions[0] = new VersionedAttributeRoutingConvention( versionedRouteName, configuration, apiVersion );
                unversionedConstraints.Add( new ODataPathRouteConstraint( versionedRouteName ) );

                var edm = model;
                var rootContainer = configuration.CreateODataRootContainer(
                                        versionedRouteName,
                                        builder => builder.AddService( Singleton, typeof( IEdmModel ), sp => edm )
                                                          .AddService( Singleton, typeof( IODataPathHandler ), sp => pathHandler )
                                                          .AddService( Singleton, typeof( IEnumerable<IODataRoutingConvention> ), sp => routeConventions.ToArray() )
                                                          .AddService( Singleton, typeof( ODataBatchHandler ), sp => batchHandler ) );

                rootContainer.InitializeAttributeRouting();

                var route = default( ODataRoute );
                var messageHandler = rootContainer.GetService<HttpMessageHandler>();
                var options = configuration.GetApiVersioningOptions();

                if ( messageHandler == null )
                {
                    route = new ODataRoute( routePrefix, routeConstraint );
                }
                else
                {
                    route = new ODataRoute( routePrefix, routeConstraint, defaults: null, constraints: null, dataTokens: null, handler: messageHandler );
                }

                routes.Add( versionedRouteName, route );
                AddApiVersionConstraintIfNecessary( route, options );
                odataRoutes.Add( route );
            }

            configuration.AddRouteToRespondWithBadRequestWhenAtLeastOneRouteCouldMatch( unversionedRouteName, routePrefix, odataRoutes, unversionedConstraints, _ => { } );

            return odataRoutes;
        }

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the model.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapVersionedODataRoute( this HttpConfiguration configuration, string routeName, string routePrefix, ApiVersion apiVersion, Action<IContainerBuilder>? configureAction ) =>
            MapVersionedODataRoute( configuration, routeName, routePrefix, apiVersion, configureAction, default );

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the model.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <param name="configureRoutingConventions">The configuring action to add or update routing conventions.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapVersionedODataRoute(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            ApiVersion apiVersion,
            Action<IContainerBuilder>? configureAction,
            Action<ODataConventionConfigurationContext>? configureRoutingConventions )
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            object ConfigureRoutingConventions( IServiceProvider serviceProvider )
            {
                var model = serviceProvider.GetRequiredService<IEdmModel>();
                var routingConventions = VersionedODataRoutingConventions.CreateDefault();
                var context = new ODataConventionConfigurationContext( configuration, routeName, model, apiVersion, routingConventions );

                model.SetAnnotationValue( model, new ApiVersionAnnotation( apiVersion ) );
                routingConventions.Insert( 0, new VersionedAttributeRoutingConvention( routeName, configuration, apiVersion ) );
                configureRoutingConventions?.Invoke( context );

                return context.RoutingConventions.ToArray();
            }

            if ( !IsNullOrEmpty( routePrefix ) )
            {
                routePrefix = routePrefix.TrimEnd( '/' );
            }

            var rootContainer = configuration.CreateODataRootContainer(
                                    routeName,
                                    builder =>
                                    {
                                        builder.AddService( Singleton, typeof( IEnumerable<IODataRoutingConvention> ), ConfigureRoutingConventions );
                                        configureAction?.Invoke( builder );
                                    } );
            var pathHandler = rootContainer.GetRequiredService<IODataPathHandler>();

            if ( pathHandler != null && pathHandler.UrlKeyDelimiter == null )
            {
                pathHandler.UrlKeyDelimiter = configuration.GetUrlKeyDelimiter();
            }

            rootContainer.InitializeAttributeRouting();

            var routeConstraint = new VersionedODataPathRouteConstraint( routeName, apiVersion );
            var route = default( ODataRoute );
            var routes = configuration.Routes;
            var messageHandler = rootContainer.GetService<HttpMessageHandler>();
            var options = configuration.GetApiVersioningOptions();

            if ( messageHandler != null )
            {
                route = new ODataRoute(
                    routePrefix,
                    routeConstraint,
                    defaults: null,
                    constraints: null,
                    dataTokens: null,
                    handler: messageHandler );
            }
            else
            {
                var batchHandler = rootContainer.GetService<ODataBatchHandler>();

                if ( batchHandler != null )
                {
                    batchHandler.ODataRouteName = routeName;
                    var batchTemplate = IsNullOrEmpty( routePrefix ) ? ODataRouteConstants.Batch : routePrefix + '/' + ODataRouteConstants.Batch;
                    routes.MapHttpBatchRoute( routeName + nameof( ODataRouteConstants.Batch ), batchTemplate, batchHandler );
                }

                route = new ODataRoute( routePrefix, routeConstraint );
            }

            routes.Add( routeName, route );
            AddApiVersionConstraintIfNecessary( route, options );

            var unversionedRouteConstraint = new ODataPathRouteConstraint( routeName );
            var unversionedRoute = new ODataRoute( routePrefix, new UnversionedODataPathRouteConstraint( unversionedRouteConstraint, apiVersion ) );

            AddApiVersionConstraintIfNecessary( unversionedRoute, options );
            configuration.Routes.Add( routeName + UnversionedRouteSuffix, unversionedRoute );

            return route;
        }

        /// <summary>
        /// Maps a versioned OData route.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to use for parsing OData paths.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the model.</param>
        /// <returns>The mapped <see cref="ODataRoute">OData route</see>.</returns>
        /// <remarks>The <see cref="ApiVersionAnnotation">API version annotation</see> will be added or updated on the specified <paramref name="model"/> using
        /// the provided <paramref name="apiVersion">API version</paramref>.</remarks>
        public static ODataRoute MapVersionedODataRoute( this HttpConfiguration configuration, string routeName, string routePrefix, IEdmModel model, ApiVersion apiVersion ) =>
            MapVersionedODataRoute( configuration, routeName, routePrefix, model, apiVersion, new DefaultODataPathHandler(), VersionedODataRoutingConventions.CreateDefault(), default, default );

        /// <summary>
        /// Maps a versioned OData route.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to use for parsing OData paths.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the model.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler">OData batch handler</see>.</param>
        /// <returns>The mapped <see cref="ODataRoute">OData route</see>.</returns>
        /// <remarks>The <see cref="ApiVersionAnnotation">API version annotation</see> will be added or updated on the specified <paramref name="model"/> using
        /// the provided <paramref name="apiVersion">API version</paramref>.</remarks>
        public static ODataRoute MapVersionedODataRoute(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEdmModel model,
            ApiVersion apiVersion,
            ODataBatchHandler? batchHandler ) =>
            MapVersionedODataRoute( configuration, routeName, routePrefix, model, apiVersion, new DefaultODataPathHandler(), VersionedODataRoutingConventions.CreateDefault(), batchHandler, default );

        /// <summary>
        /// Maps the specified OData route and the OData route attributes. When the <paramref name="defaultHandler"/>
        /// is non-<c>null</c>, it will map it as the default handler for the route.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the model.</param>
        /// <param name="defaultHandler">The default <see cref="HttpMessageHandler"/> for this route.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapVersionedODataRoute(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEdmModel model,
            ApiVersion apiVersion,
            HttpMessageHandler? defaultHandler ) =>
            MapVersionedODataRoute( configuration, routeName, routePrefix, model, apiVersion, new DefaultODataPathHandler(), VersionedODataRoutingConventions.CreateDefault(), default, defaultHandler );

        /// <summary>
        /// Maps a versioned OData route.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to use for parsing OData paths.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the model.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler">OData path handler</see> to use for parsing the OData path.</param>
        /// <param name="routingConventions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IODataRoutingConvention">OData routing conventions</see>
        /// to use for controller and action selection.</param>
        /// <returns>The mapped <see cref="ODataRoute">OData route</see>.</returns>
        /// <remarks>The <see cref="ApiVersionAnnotation">API version annotation</see> will be added or updated on the specified <paramref name="model"/> using
        /// the provided <paramref name="apiVersion">API version</paramref>.</remarks>
        public static ODataRoute MapVersionedODataRoute(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEdmModel model,
            ApiVersion apiVersion,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions ) =>
            MapVersionedODataRoute( configuration, routeName, routePrefix, model, apiVersion, pathHandler, routingConventions, default, default );

        /// <summary>
        /// Maps a versioned OData route. When the <paramref name="batchHandler"/> is provided, it will create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to use for parsing OData paths.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the model.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler">OData path handler</see> to use for parsing the OData path.</param>
        /// <param name="routingConventions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IODataRoutingConvention">OData routing conventions</see>
        /// to use for controller and action selection.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler">OData batch handler</see>.</param>
        /// <returns>The mapped <see cref="ODataRoute">OData route</see>.</returns>
        /// <remarks>The <see cref="ApiVersionAnnotation">API version annotation</see> will be added or updated on the specified <paramref name="model"/> using
        /// the provided <paramref name="apiVersion">API version</paramref>.</remarks>
        public static ODataRoute MapVersionedODataRoute(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEdmModel model,
            ApiVersion apiVersion,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions,
            ODataBatchHandler? batchHandler ) =>
            MapVersionedODataRoute( configuration, routeName, routePrefix, model, apiVersion, pathHandler, routingConventions, batchHandler, default );

        /// <summary>
        /// Maps the specified OData route. When the <paramref name="defaultHandler"/> is non-<c>null</c>, it will map
        /// it as the handler for the route.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the model.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler" /> to use for parsing the OData path.</param>
        /// <param name="routingConventions">The OData routing conventions to use for controller and action selection.</param>
        /// <param name="defaultHandler">The default <see cref="HttpMessageHandler"/> for this route.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapVersionedODataRoute(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEdmModel model,
            ApiVersion apiVersion,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions,
            HttpMessageHandler? defaultHandler ) =>
            MapVersionedODataRoute( configuration, routeName, routePrefix, model, apiVersion, pathHandler, routingConventions, default, defaultHandler );

        /// <summary>
        /// Gets the configured entity data model (EDM) for the specified API version.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to get the model for.</param>
        /// <returns>The matching <see cref="IEdmModel">EDM model</see> or <c>null</c>.</returns>
        public static IEdmModel? GetEdmModel( this HttpConfiguration configuration, ApiVersion apiVersion )
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            var routes = configuration.Routes.ToDictionary();
            var containers = configuration.GetRootContainerMappings();

            foreach ( var route in routes )
            {
                if ( !( route.Value is ODataRoute odataRoute ) )
                {
                    continue;
                }

                if ( !containers.TryGetValue( route.Key, out var serviceProvider ) )
                {
                    continue;
                }

                var model = serviceProvider.GetService<IEdmModel>();

                if ( model?.EntityContainer == null )
                {
                    continue;
                }

                var modelApiVersion = model.GetAnnotationValue<ApiVersionAnnotation>( model )?.ApiVersion;

                if ( modelApiVersion == apiVersion )
                {
                    return model;
                }
            }

            return null;
        }

        internal static IServiceProvider GetODataRootContainer( this HttpConfiguration configuration, string routeName ) => configuration.GetRootContainerMappings()[routeName];

        internal static ODataUrlKeyDelimiter? GetUrlKeyDelimiter( this HttpConfiguration configuration )
        {
            if ( configuration.Properties.TryGetValue( UrlKeyDelimiterKey, out var value ) )
            {
                return value as ODataUrlKeyDelimiter;
            }

            configuration.Properties[UrlKeyDelimiterKey] = null;
            return null;
        }

        static ODataRoute MapVersionedODataRoute(
            HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEdmModel model,
            ApiVersion apiVersion,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions,
            ODataBatchHandler? batchHandler,
            HttpMessageHandler? defaultHandler )
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException( nameof( configuration ) );
            }

            var routeConventions = VersionedODataRoutingConventions.AddOrUpdate( routingConventions.ToList() );
            var routes = configuration.Routes;

            if ( !IsNullOrEmpty( routePrefix ) )
            {
                routePrefix = routePrefix.TrimEnd( '/' );
            }

            if ( pathHandler != null && pathHandler.UrlKeyDelimiter == null )
            {
                pathHandler.UrlKeyDelimiter = configuration.GetUrlKeyDelimiter();
            }

            model.SetAnnotationValue( model, new ApiVersionAnnotation( apiVersion ) );
            routeConventions.Insert( 0, new VersionedAttributeRoutingConvention( routeName, configuration, apiVersion ) );

            var rootContainer = configuration.CreateODataRootContainer(
                                    routeName,
                                    builder => builder.AddService( Singleton, typeof( IEdmModel ), sp => model )
                                                      .AddService( Singleton, typeof( IODataPathHandler ), sp => pathHandler )
                                                      .AddService( Singleton, typeof( IEnumerable<IODataRoutingConvention> ), sp => routeConventions.ToArray() )
                                                      .AddService( Singleton, typeof( ODataBatchHandler ), sp => batchHandler )
                                                      .AddService( Singleton, typeof( HttpMessageHandler ), sp => defaultHandler ) );

            rootContainer.InitializeAttributeRouting();

            var routeConstraint = new VersionedODataPathRouteConstraint( routeName, apiVersion );
            var route = default( ODataRoute );
            var options = configuration.GetApiVersioningOptions();

            if ( defaultHandler != null )
            {
                route = new ODataRoute( routePrefix, routeConstraint, defaults: null, constraints: null, dataTokens: null, handler: defaultHandler );
            }
            else
            {
                if ( batchHandler != null )
                {
                    batchHandler.ODataRouteName = routeName;
                    var batchTemplate = IsNullOrEmpty( routePrefix ) ? ODataRouteConstants.Batch : routePrefix + '/' + ODataRouteConstants.Batch;
                    routes.MapHttpBatchRoute( routeName + nameof( ODataRouteConstants.Batch ), batchTemplate, batchHandler );
                }

                route = new ODataRoute( routePrefix, routeConstraint );
            }

            routes.Add( routeName, route );
            AddApiVersionConstraintIfNecessary( route, options );

            var unversionedRouteConstraint = new ODataPathRouteConstraint( routeName );
            var unversionedRoute = new ODataRoute( routePrefix, new UnversionedODataPathRouteConstraint( unversionedRouteConstraint, apiVersion ) );

            AddApiVersionConstraintIfNecessary( unversionedRoute, options );
            routes.Add( routeName + UnversionedRouteSuffix, unversionedRoute );

            return route;
        }

        static ODataPathRouteConstraint MakeVersionedODataRouteConstraint( ApiVersion? apiVersion, ref string versionedRouteName )
        {
            if ( apiVersion == null )
            {
                return new ODataPathRouteConstraint( versionedRouteName );
            }

            versionedRouteName += "-" + apiVersion.ToString();
            return new VersionedODataPathRouteConstraint( versionedRouteName, apiVersion );
        }

        static void AddApiVersionConstraintIfNecessary( ODataRoute route, ApiVersioningOptions options )
        {
            var routePrefix = route.RoutePrefix;
            var apiVersionConstraint = "{" + options.RouteConstraintName + "}";

            if ( routePrefix == null || routePrefix.IndexOf( apiVersionConstraint, Ordinal ) < 0 || route.Constraints.ContainsKey( options.RouteConstraintName ) )
            {
                return;
            }

            // note: even though the constraints are a dictionary, it's important to rebuild the entire collection
            // to make sure the api version constraint is evaluated first; otherwise, the current api version will
            // not be resolved when the odata versioning constraint is evaluated
            var originalConstraints = new Dictionary<string, object>( route.Constraints );

            route.Constraints.Clear();
            route.Constraints.Add( options.RouteConstraintName, new ApiVersionRouteConstraint() );

            foreach ( var constraint in originalConstraints )
            {
                route.Constraints.Add( constraint.Key, constraint.Value );
            }
        }

        static void AddRouteToRespondWithBadRequestWhenAtLeastOneRouteCouldMatch(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            List<ODataRoute> odataRoutes,
            List<IHttpRouteConstraint> unversionedConstraints,
            Action<IContainerBuilder>? configureAction )
        {
            var unversionedRoute = new ODataRoute( routePrefix, new UnversionedODataPathRouteConstraint( unversionedConstraints ) );
            var options = configuration.GetApiVersioningOptions();

            AddApiVersionConstraintIfNecessary( unversionedRoute, options );
            configuration.Routes.Add( routeName, unversionedRoute );
            odataRoutes.Add( unversionedRoute );
            configuration.CreateODataRootContainer( routeName, configureAction );
        }

        static IServiceProvider CreateODataRootContainer( this HttpConfiguration configuration, string routeName, Action<IContainerBuilder>? configureAction )
        {
            var rootContainer = configuration.CreateRootContainerImplementation( configureAction );
            configuration.SetODataRootContainer( routeName, rootContainer );
            return rootContainer;
        }

        static void SetODataRootContainer( this HttpConfiguration configuration, string routeName, IServiceProvider rootContainer ) =>
            configuration.GetRootContainerMappings()[routeName] = rootContainer;

        static ConcurrentDictionary<string, IServiceProvider> GetRootContainerMappings( this HttpConfiguration configuration ) =>
            (ConcurrentDictionary<string, IServiceProvider>) configuration.Properties.GetOrAdd( RootContainerMappingsKey, key => new ConcurrentDictionary<string, IServiceProvider>() );

        static IServiceProvider CreateRootContainerImplementation( this HttpConfiguration configuration, Action<IContainerBuilder>? configureAction )
        {
            var builder = configuration.CreateContainerBuilderWithDefaultServices();

            configureAction?.Invoke( builder );

            var rootContainer = builder.BuildContainer();

            if ( rootContainer == null )
            {
                throw new InvalidOperationException( SR.NullContainer );
            }

            return rootContainer;
        }

        static IContainerBuilder CreateContainerBuilderWithDefaultServices( this HttpConfiguration configuration )
        {
            IContainerBuilder builder;

            if ( configuration.Properties.TryGetValue( ContainerBuilderFactoryKey, out var value ) )
            {
                var builderFactory = (Func<IContainerBuilder>) value;

                builder = builderFactory();

                if ( builder == null )
                {
                    throw new InvalidOperationException( SR.NullContainerBuilder );
                }
            }
            else
            {
                builder = new DefaultContainerBuilder();
            }

            builder.AddService( Singleton, sp => configuration );
            builder.AddService( Singleton, sp => configuration.GetDefaultQuerySettings() );
            builder.AddDefaultODataServices();
            builder.AddDefaultWebApiServices();

            return builder;
        }
    }
}