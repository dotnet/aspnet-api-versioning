namespace Microsoft.AspNet.OData.Extensions
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Microsoft.OData.ServiceLifetime;
    using static System.Reflection.BindingFlags;
    using static System.String;

    /// <summary>
    /// Provides extension methods for <see cref="IRouteBuilder"/> to add versioned OData routes.
    /// </summary>
    [CLSCompliant( false )]
    public static class IRouteBuilderExtensions
    {
        const string UnversionedRouteSuffix = "-Unversioned";
        static readonly Func<IRouteBuilder, Action<IContainerBuilder>, Action<IContainerBuilder>> configureDefaultServicesFunc = ResolveConfigureDefaultServicesFunc();

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            Action<IContainerBuilder> configureAction ) =>
            MapVersionedODataRoutes( builder, routeName, routePrefix, models, configureAction, default );

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
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
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            Action<IContainerBuilder>? configureAction,
            Action<ODataConventionConfigurationContext>? configureRoutingConventions )
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            if ( models == null )
            {
                throw new ArgumentNullException( nameof( models ) );
            }

            IEnumerable<IODataRoutingConvention> ConfigureRoutingConventions( IEdmModel model, string versionedRouteName, ApiVersion apiVersion )
            {
                var routingConventions = VersionedODataRoutingConventions.CreateDefault();
                var context = new ODataConventionConfigurationContext( versionedRouteName, model, apiVersion, routingConventions );

                model.SetAnnotationValue( model, new ApiVersionAnnotation( apiVersion ) );
                routingConventions.Insert( 0, new VersionedAttributeRoutingConvention( versionedRouteName, builder.ServiceProvider, apiVersion ) );
                configureRoutingConventions?.Invoke( context );

                return context.RoutingConventions;
            }

            builder.EnsureMetadataController();

            var routeCollection = builder.ServiceProvider.GetRequiredService<IODataRouteCollectionProvider>();
            var perRouteContainer = builder.ServiceProvider.GetRequiredService<IPerRouteContainer>();
            var options = builder.ServiceProvider.GetRequiredService<ODataOptions>();
            var inlineConstraintResolver = builder.ServiceProvider.GetRequiredService<IInlineConstraintResolver>();
            var routes = builder.Routes;
            var odataRoutes = new List<ODataRoute>();
            var unversionedConstraints = new List<IRouteConstraint>();

            foreach ( var model in models )
            {
                var versionedRouteName = routeName;
                var annotation = model.GetAnnotationValue<ApiVersionAnnotation>( model ) ?? throw new ArgumentException( LocalSR.MissingAnnotation.FormatDefault( typeof( ApiVersionAnnotation ).Name ) );
                var apiVersion = annotation.ApiVersion;
                var routeConstraint = MakeVersionedODataRouteConstraint( apiVersion, ref versionedRouteName );
                var preConfigureAction = builder.ConfigureDefaultServices(
                    container =>
                    {
                        container.AddService( Singleton, typeof( IEdmModel ), sp => model )
                                 .AddService( Singleton, typeof( IEnumerable<IODataRoutingConvention> ), sp => ConfigureRoutingConventions( model, versionedRouteName, apiVersion ) );
                        configureAction?.Invoke( container );
                    } );
                var rootContainer = perRouteContainer.CreateODataRootContainer( versionedRouteName, preConfigureAction );
                var router = rootContainer.GetService<IRouter>() ?? builder.DefaultHandler;

                rootContainer.ConfigurePathHandler( options );

                var route = new ODataRoute( router, versionedRouteName, routePrefix.RemoveTrailingSlash(), routeConstraint, inlineConstraintResolver );

                unversionedConstraints.Add( new ODataPathRouteConstraint( versionedRouteName ) );
                builder.ConfigureBatchHandler( rootContainer, route );
                routes.Add( route );
                odataRoutes.Add( route );
                routeCollection.Add( new ODataRouteMapping( route, apiVersion, rootContainer ) );
            }

            builder.AddRouteToRespondWithBadRequestWhenAtLeastOneRouteCouldMatch( routeName, routePrefix, unversionedConstraints, inlineConstraintResolver );
            NotifyRoutesMapped();

            return odataRoutes;
        }

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes( this IRouteBuilder builder, string routeName, string routePrefix, IEnumerable<IEdmModel> models ) =>
            MapVersionedODataRoutes( builder, routeName, routePrefix, models, new DefaultODataPathHandler(), VersionedODataRoutingConventions.CreateDefault(), default );

        /// <summary>
        /// Maps the specified versioned OData routes. When the <paramref name="newBatchHandler"/> is provided, it will create a
        /// '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="newBatchHandler">The <see cref="Func{TResult}">factory method</see> used to create new <see cref="ODataBatchHandler">OData batch handlers</see>.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            Func<ODataBatchHandler>? newBatchHandler ) =>
            MapVersionedODataRoutes( builder, routeName, routePrefix, models, new DefaultODataPathHandler(), VersionedODataRoutingConventions.CreateDefault(), newBatchHandler );

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
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
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions ) =>
            MapVersionedODataRoutes( builder, routeName, routePrefix, models, pathHandler, routingConventions, default );

        /// <summary>
        /// Maps the specified versioned OData routes. When the <paramref name="newBatchHandler"/> is provided, it will create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler">OData path handler</see> to use for parsing the OData path.</param>
        /// <param name="routingConventions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IODataRoutingConvention">OData routing conventions</see>
        /// to use for controller and action selection.</param>
        /// <param name="newBatchHandler">The <see cref="Func{TResult}">factory method</see> used to create new <see cref="ODataBatchHandler">OData batch handlers</see>.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions,
            Func<ODataBatchHandler>? newBatchHandler )
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            if ( models == null )
            {
                throw new ArgumentNullException( nameof( models ) );
            }

            var serviceProvider = builder.ServiceProvider;
            var options = serviceProvider.GetRequiredService<ODataOptions>();
            var routeCollection = serviceProvider.GetRequiredService<IODataRouteCollectionProvider>();
            var inlineConstraintResolver = serviceProvider.GetRequiredService<IInlineConstraintResolver>();
            var routeConventions = VersionedODataRoutingConventions.AddOrUpdate( routingConventions.ToList() );
            var routes = builder.Routes;
            var perRouteContainer = serviceProvider.GetRequiredService<IPerRouteContainer>();
            var odataRoutes = new List<ODataRoute>();
            var unversionedConstraints = new List<IRouteConstraint>();

            if ( pathHandler != null && pathHandler.UrlKeyDelimiter == null )
            {
                pathHandler.UrlKeyDelimiter = options.UrlKeyDelimiter;
            }

            foreach ( var model in models )
            {
                var versionedRouteName = routeName;
                var annotation = model.GetAnnotationValue<ApiVersionAnnotation>( model ) ?? throw new ArgumentException( LocalSR.MissingAnnotation.FormatDefault( typeof( ApiVersionAnnotation ).Name ) );
                var apiVersion = annotation.ApiVersion;
                var routeConstraint = MakeVersionedODataRouteConstraint( apiVersion, ref versionedRouteName );

                IEnumerable<IODataRoutingConvention> NewRouteConventions( IServiceProvider services )
                {
                    var conventions = new IODataRoutingConvention[routeConventions.Count + 1];
                    conventions[0] = new VersionedAttributeRoutingConvention( versionedRouteName, serviceProvider, apiVersion );
                    routeConventions.CopyTo( conventions, 1 );
                    return conventions;
                }

                var edm = model;
                var batchHandler = newBatchHandler?.Invoke();
                var configureAction = builder.ConfigureDefaultServices( container =>
                    container.AddService( Singleton, typeof( IEdmModel ), sp => edm )
                             .AddService( Singleton, typeof( IODataPathHandler ), sp => pathHandler )
                             .AddService( Singleton, typeof( IEnumerable<IODataRoutingConvention> ), NewRouteConventions )
                             .AddService( Singleton, typeof( ODataBatchHandler ), sp => batchHandler ) );
                var rootContainer = perRouteContainer.CreateODataRootContainer( versionedRouteName, configureAction );
                var router = rootContainer.GetService<IRouter>() ?? builder.DefaultHandler;
                var route = new ODataRoute( router, versionedRouteName, routePrefix.RemoveTrailingSlash(), routeConstraint, inlineConstraintResolver );

                unversionedConstraints.Add( new ODataPathRouteConstraint( versionedRouteName ) );
                builder.ConfigureBatchHandler( batchHandler, route );
                routes.Add( route );
                odataRoutes.Add( route );
                routeCollection.Add( new ODataRouteMapping( route, apiVersion, rootContainer ) );
            }

            builder.AddRouteToRespondWithBadRequestWhenAtLeastOneRouteCouldMatch( routeName, routePrefix, unversionedConstraints, inlineConstraintResolver );
            NotifyRoutesMapped();

            return odataRoutes;
        }

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the model.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapVersionedODataRoute(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            ApiVersion apiVersion,
            Action<IContainerBuilder>? configureAction ) =>
            MapVersionedODataRoute( builder, routeName, routePrefix, apiVersion, configureAction, default );

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the model.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <param name="configureRoutingConventions">The configuring action to add or update routing conventions.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapVersionedODataRoute(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            ApiVersion apiVersion,
            Action<IContainerBuilder>? configureAction,
            Action<ODataConventionConfigurationContext>? configureRoutingConventions )
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            IEnumerable<IODataRoutingConvention> NewRoutingConventions( IServiceProvider serviceProvider )
            {
                var model = serviceProvider.GetRequiredService<IEdmModel>();
                var routingConventions = VersionedODataRoutingConventions.CreateDefault();
                var context = new ODataConventionConfigurationContext( routeName, model, apiVersion, routingConventions );

                model.SetAnnotationValue( model, new ApiVersionAnnotation( apiVersion ) );
                routingConventions.Insert( 0, new VersionedAttributeRoutingConvention( routeName, builder.ServiceProvider, apiVersion ) );
                configureRoutingConventions?.Invoke( context );

                return context.RoutingConventions.ToArray();
            }

            var routeCollection = builder.ServiceProvider.GetRequiredService<IODataRouteCollectionProvider>();
            var perRouteContainer = builder.ServiceProvider.GetRequiredService<IPerRouteContainer>();
            var inlineConstraintResolver = builder.ServiceProvider.GetRequiredService<IInlineConstraintResolver>();
            var preConfigureAction = builder.ConfigureDefaultServices(
                container =>
                {
                    container.AddService( Singleton, typeof( IEnumerable<IODataRoutingConvention> ), NewRoutingConventions );
                    configureAction?.Invoke( container );
                } );
            var rootContainer = perRouteContainer.CreateODataRootContainer( routeName, preConfigureAction );
            var router = rootContainer.GetService<IRouter>() ?? builder.DefaultHandler;

            builder.ConfigurePathHandler( rootContainer );

            var routeConstraint = new VersionedODataPathRouteConstraint( routeName, apiVersion );
            var route = new ODataRoute( router, routeName, routePrefix.RemoveTrailingSlash(), routeConstraint, inlineConstraintResolver );

            builder.ConfigureBatchHandler( rootContainer, route );
            builder.Routes.Add( route );
            routeCollection.Add( new ODataRouteMapping( route, apiVersion, rootContainer ) );
            builder.AddRouteToRespondWithBadRequestWhenAtLeastOneRouteCouldMatch( routeName, routePrefix, apiVersion, inlineConstraintResolver );
            NotifyRoutesMapped();

            return route;
        }

        /// <summary>
        /// Maps a versioned OData route.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to use for parsing OData paths.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the model.</param>
        /// <returns>The mapped <see cref="ODataRoute">OData route</see>.</returns>
        /// <remarks>The <see cref="ApiVersionAnnotation">API version annotation</see> will be added or updated on the specified <paramref name="model"/> using
        /// the provided <paramref name="apiVersion">API version</paramref>.</remarks>
        public static ODataRoute MapVersionedODataRoute( this IRouteBuilder builder, string routeName, string routePrefix, IEdmModel model, ApiVersion apiVersion ) =>
            MapVersionedODataRoute( builder, routeName, routePrefix, model, apiVersion, new DefaultODataPathHandler(), VersionedODataRoutingConventions.CreateDefault(), default );

        /// <summary>
        /// Maps a versioned OData route.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to use for parsing OData paths.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the model.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler">OData batch handler</see>.</param>
        /// <returns>The mapped <see cref="ODataRoute">OData route</see>.</returns>
        /// <remarks>The <see cref="ApiVersionAnnotation">API version annotation</see> will be added or updated on the specified <paramref name="model"/> using
        /// the provided <paramref name="apiVersion">API version</paramref>.</remarks>
        public static ODataRoute MapVersionedODataRoute(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEdmModel model,
            ApiVersion apiVersion,
            ODataBatchHandler? batchHandler ) =>
            MapVersionedODataRoute( builder, routeName, routePrefix, model, apiVersion, new DefaultODataPathHandler(), VersionedODataRoutingConventions.CreateDefault(), batchHandler );

        /// <summary>
        /// Maps a versioned OData route.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
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
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEdmModel model,
            ApiVersion apiVersion,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions ) =>
            MapVersionedODataRoute( builder, routeName, routePrefix, model, apiVersion, pathHandler, routingConventions, default );

        /// <summary>
        /// Maps a versioned OData route. When the <paramref name="batchHandler"/> is provided, it will create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
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
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEdmModel model,
            ApiVersion apiVersion,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions,
            ODataBatchHandler? batchHandler )
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            IEnumerable<IODataRoutingConvention> NewRoutingConventions( IServiceProvider serviceProvider )
            {
                var conventions = VersionedODataRoutingConventions.AddOrUpdate( routingConventions.ToList() );
                conventions.Insert( 0, new VersionedAttributeRoutingConvention( routeName, builder.ServiceProvider, apiVersion ) );
                return conventions.ToArray();
            }

            var routeCollection = builder.ServiceProvider.GetRequiredService<IODataRouteCollectionProvider>();
            var perRouteContainer = builder.ServiceProvider.GetRequiredService<IPerRouteContainer>();
            var options = builder.ServiceProvider.GetRequiredService<ODataOptions>();
            var inlineConstraintResolver = builder.ServiceProvider.GetRequiredService<IInlineConstraintResolver>();

            if ( pathHandler != null && pathHandler.UrlKeyDelimiter == null )
            {
                pathHandler.UrlKeyDelimiter = options.UrlKeyDelimiter;
            }

            model.SetAnnotationValue( model, new ApiVersionAnnotation( apiVersion ) );

            var configureAction = builder.ConfigureDefaultServices( container =>
                    container.AddService( Singleton, typeof( IEdmModel ), sp => model )
                             .AddService( Singleton, typeof( IODataPathHandler ), sp => pathHandler )
                             .AddService( Singleton, typeof( IEnumerable<IODataRoutingConvention> ), NewRoutingConventions )
                             .AddService( Singleton, typeof( ODataBatchHandler ), sp => batchHandler ) );
            var rootContainer = perRouteContainer.CreateODataRootContainer( routeName, configureAction );
            var router = rootContainer.GetService<IRouter>() ?? builder.DefaultHandler;
            var routeConstraint = new VersionedODataPathRouteConstraint( routeName, apiVersion );
            var route = new ODataRoute( router, routeName, routePrefix.RemoveTrailingSlash(), routeConstraint, inlineConstraintResolver );

            builder.ConfigureBatchHandler( rootContainer, route );
            builder.Routes.Add( route );
            routeCollection.Add( new ODataRouteMapping( route, apiVersion, rootContainer ) );
            builder.AddRouteToRespondWithBadRequestWhenAtLeastOneRouteCouldMatch( routeName, routePrefix, apiVersion, inlineConstraintResolver );
            NotifyRoutesMapped();

            return route;
        }

        static Action<IContainerBuilder> ConfigureDefaultServices( this IRouteBuilder builder, Action<IContainerBuilder> configureAction ) => configureDefaultServicesFunc( builder, configureAction );

        static Func<IRouteBuilder, Action<IContainerBuilder>, Action<IContainerBuilder>> ResolveConfigureDefaultServicesFunc()
        {
            var method = typeof( ODataRouteBuilderExtensions ).GetMethod( "ConfigureDefaultServices", NonPublic | Static, null, new[] { typeof( IRouteBuilder ), typeof( Action<IContainerBuilder> ) }, null )!;
            return (Func<IRouteBuilder, Action<IContainerBuilder>, Action<IContainerBuilder>>) method.CreateDelegate( typeof( Func<IRouteBuilder, Action<IContainerBuilder>, Action<IContainerBuilder>> ) );
        }

        static void EnsureMetadataController( this IRouteBuilder builder )
        {
            var applicationPartManager = builder.ServiceProvider.GetRequiredService<ApplicationPartManager>();
            applicationPartManager.ApplicationParts.Add( new AssemblyPart( typeof( VersionedMetadataController ).Assembly ) );
        }

        static void ConfigurePathHandler( this IRouteBuilder builder, IServiceProvider rootContainer )
        {
            var options = builder.ServiceProvider.GetRequiredService<ODataOptions>();
            rootContainer.ConfigurePathHandler( options );
        }

        static void ConfigurePathHandler( this IServiceProvider rootContainer, ODataOptions options )
        {
            var pathHandler = rootContainer.GetRequiredService<IODataPathHandler>();

            if ( pathHandler != null && pathHandler.UrlKeyDelimiter == null )
            {
                pathHandler.UrlKeyDelimiter = options.UrlKeyDelimiter;
            }
        }

        static void ConfigureBatchHandler( this IRouteBuilder builder, IServiceProvider rootContainer, ODataRoute route )
        {
            if ( rootContainer.GetService<ODataBatchHandler>() is ODataBatchHandler batchHandler )
            {
                batchHandler.Configure( builder, route );
            }
        }

        static void ConfigureBatchHandler( this IRouteBuilder builder, ODataBatchHandler? batchHandler, ODataRoute route ) => batchHandler?.Configure( builder, route );

        static void Configure( this ODataBatchHandler batchHandler, IRouteBuilder builder, ODataRoute route )
        {
            batchHandler.ODataRoute = route;
            batchHandler.ODataRouteName = route.Name;

            var batchPath = '/' + ODataRouteConstants.Batch;

            if ( !IsNullOrEmpty( route.RoutePrefix ) )
            {
                batchPath = '/' + route.RoutePrefix + batchPath;
            }

            var batchMapping = builder.ServiceProvider.GetRequiredService<ODataBatchPathMapping>();

            batchMapping.AddRoute( route.Name, batchPath );
        }

        static void AddRouteToRespondWithBadRequestWhenAtLeastOneRouteCouldMatch(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IRouteConstraint> unversionedConstraints,
            IInlineConstraintResolver inlineConstraintResolver )
        {
            routeName += UnversionedRouteSuffix;

            var constraint = new UnversionedODataPathRouteConstraint( unversionedConstraints );
            var route = new ODataRoute( builder.DefaultHandler, routeName, routePrefix, constraint, inlineConstraintResolver );

            builder.Routes.Add( route );
        }

        static void AddRouteToRespondWithBadRequestWhenAtLeastOneRouteCouldMatch(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            ApiVersion apiVersion,
            IInlineConstraintResolver inlineConstraintResolver )
        {
            routeName += UnversionedRouteSuffix;

            var innerConstraint = new ODataPathRouteConstraint( routeName );
            var constraint = new UnversionedODataPathRouteConstraint( innerConstraint, apiVersion );
            var route = new ODataRoute( builder.DefaultHandler, routeName, routePrefix, constraint, inlineConstraintResolver );

            builder.Routes.Add( route );
        }

        static IRouteConstraint MakeVersionedODataRouteConstraint( ApiVersion apiVersion, ref string versionedRouteName )
        {
            if ( apiVersion == null )
            {
                return new ODataPathRouteConstraint( versionedRouteName );
            }

            versionedRouteName += "-" + apiVersion.ToString();
            return new VersionedODataPathRouteConstraint( versionedRouteName, apiVersion );
        }

        static string RemoveTrailingSlash( this string @string ) => IsNullOrEmpty( @string ) ? @string : @string.TrimEnd( '/' );

        // note: we don't have the required information necessary to build the odata route information
        // until one or more routes have been mapped. if anyone has subscribed changes, notify them now.
        static void NotifyRoutesMapped() => ODataActionDescriptorChangeProvider.Instance.NotifyChanged();
    }
}