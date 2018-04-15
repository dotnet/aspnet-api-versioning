namespace Microsoft.AspNet.OData.Extensions
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using Microsoft.Web.OData.Routing;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.Contracts;
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
        const string ApiVersionConstraintName = "apiVersion";
        const string ApiVersionConstraint = "{" + ApiVersionConstraintName + "}";
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
            Action<IContainerBuilder> configureAction )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( routeName, nameof( routeName ) );
            Arg.NotNull( models, nameof( models ) );
            Contract.Ensures( Contract.Result<IReadOnlyList<ODataRoute>>() != null );

            IEnumerable<IODataRoutingConvention> ConfigureRoutingConventions( IEdmModel model, string versionedRouteName, ApiVersion apiVersion )
            {
                var routingConventions = EnsureConventions( ODataRoutingConventions.CreateDefault() );
                model.SetAnnotationValue( model, new ApiVersionAnnotation( apiVersion ) );
                return routingConventions;
            }

            if ( !IsNullOrEmpty( routePrefix ) )
            {
                routePrefix = routePrefix.TrimEnd( '/' );
            }

            var perRouteContainer = builder.ServiceProvider.GetRequiredService<IPerRouteContainer>();
            var options = builder.ServiceProvider.GetRequiredService<ODataOptions>();
            var inlineConstraintResolver = builder.ServiceProvider.GetRequiredService<IInlineConstraintResolver>();
            var routes = builder.Routes;
            var unversionedRouteName = routeName + UnversionedRouteSuffix;
            var odataRoutes = new List<ODataRoute>();
            var unversionedConstraints = new List<IRouteConstraint>();

            foreach ( var model in models )
            {
                var versionedRouteName = routeName;
                var apiVersion = model.GetAnnotationValue<ApiVersionAnnotation>( model )?.ApiVersion;
                var routeConstraint = MakeVersionedODataRouteConstraint( apiVersion, ref versionedRouteName );

                unversionedConstraints.Add( new ODataPathRouteConstraint( versionedRouteName ) );

                var preConfigureAction = builder.ConfigureDefaultServices(
                    container =>
                    {
                        container.AddService( Singleton, typeof( IEdmModel ), sp => model )
                                 .AddService( Singleton, typeof( IEnumerable<IODataRoutingConvention> ), sp => ConfigureRoutingConventions( model, versionedRouteName, apiVersion ) );
                        configureAction?.Invoke( container );
                    } );
                var rootContainer = perRouteContainer.CreateODataRootContainer( versionedRouteName, preConfigureAction );
                var pathHandler = rootContainer.GetRequiredService<IODataPathHandler>();
                var router = rootContainer.GetService<IRouter>() ?? builder.DefaultHandler;

                if ( pathHandler != null && pathHandler.UrlKeyDelimiter == null )
                {
                    pathHandler.UrlKeyDelimiter = options.UrlKeyDelimiter;
                }

                var route = new ODataRoute( router, versionedRouteName, routePrefix, routeConstraint, inlineConstraintResolver );

                // TODO: review. this appears to be wrong. there should be only one batch handler per application
                if ( rootContainer.GetService<ODataBatchHandler>() is ODataBatchHandler batchHandler )
                {
                    batchHandler.ODataRoute = route;
                    batchHandler.ODataRouteName = versionedRouteName;

                    var batchPath = IsNullOrEmpty( routePrefix ) ? '/' + ODataRouteConstants.Batch : '/' + routePrefix + '/' + ODataRouteConstants.Batch;
                    var batchMapping = builder.ServiceProvider.GetRequiredService<ODataBatchPathMapping>();

                    batchMapping.AddRoute( versionedRouteName, batchPath );
                }

                routes.Add( route );
                odataRoutes.Add( route );
            }

            // TODO: do we still need to handle unmatched conventions? how?
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
            MapVersionedODataRoutes( builder, routeName, routePrefix, models, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault(), null );

        /// <summary>
        /// Maps the specified versioned OData routes. When the <paramref name="batchHandler"/> is provided, it will create a
        /// '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler">OData batch handler</see>.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            ODataBatchHandler batchHandler ) =>
            MapVersionedODataRoutes( builder, routeName, routePrefix, models, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault(), batchHandler );

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
            MapVersionedODataRoutes( builder, routeName, routePrefix, models, pathHandler, routingConventions, null );

        /// <summary>
        /// Maps the specified versioned OData routes. When the <paramref name="batchHandler"/> is provided, it will create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
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
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions,
            ODataBatchHandler batchHandler )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( routeName, nameof( routeName ) );
            Arg.NotNull( models, nameof( models ) );
            Contract.Ensures( Contract.Result<IReadOnlyList<ODataRoute>>() != null );

            var options = builder.ServiceProvider.GetRequiredService<ODataOptions>();
            var inlineConstraintResolver = builder.ServiceProvider.GetRequiredService<IInlineConstraintResolver>();
            var routeConventions = EnsureConventions( routingConventions.ToList() );
            var routes = builder.Routes;
            var unversionedRouteName = routeName + UnversionedRouteSuffix;
            var perRouteContainer = builder.ServiceProvider.GetRequiredService<IPerRouteContainer>();

            if ( !IsNullOrEmpty( routePrefix ) )
            {
                routePrefix = routePrefix.TrimEnd( '/' );
            }

            // TODO: what's the right way to set this up?
            // if ( batchHandler != null )
            // {
            //     batchHandler.ODataRouteName = unversionedRouteName;
            //     var batchTemplate = IsNullOrEmpty( routePrefix ) ? ODataRouteConstants.Batch : routePrefix + '/' + ODataRouteConstants.Batch;
            //     routes.MapHttpBatchRoute( routeName + nameof( ODataRouteConstants.Batch ), batchTemplate, batchHandler );
            // }
            if ( pathHandler != null && pathHandler.UrlKeyDelimiter == null )
            {
                pathHandler.UrlKeyDelimiter = options.UrlKeyDelimiter;
            }

            var odataRoutes = new List<ODataRoute>();
            var unversionedConstraints = new List<IRouteConstraint>();

            foreach ( var model in models )
            {
                var versionedRouteName = routeName;
                var apiVersion = model.GetAnnotationValue<ApiVersionAnnotation>( model )?.ApiVersion;
                var routeConstraint = MakeVersionedODataRouteConstraint( apiVersion, ref versionedRouteName );

                IEnumerable<IODataRoutingConvention> NewRouteConventions( IServiceProvider serviceProvider )
                {
                    var conventions = new IODataRoutingConvention[routeConventions.Count + 1];
                    conventions[0] = new VersionedAttributeRoutingConvention( versionedRouteName, builder.ServiceProvider, apiVersion );
                    routeConventions.CopyTo( conventions, 0 );
                    return conventions;
                }

                unversionedConstraints.Add( new ODataPathRouteConstraint( versionedRouteName ) );

                var edm = model;
                var configureAction = builder.ConfigureDefaultServices( container =>
                    container.AddService( Singleton, typeof( IEdmModel ), sp => edm )
                             .AddService( Singleton, typeof( IODataPathHandler ), sp => pathHandler )
                             .AddService( Singleton, typeof( IEnumerable<IODataRoutingConvention> ), NewRouteConventions )
                             .AddService( Singleton, typeof( ODataBatchHandler ), sp => batchHandler ) );
                var rootContainer = perRouteContainer.CreateODataRootContainer( versionedRouteName, configureAction );
                var router = rootContainer.GetService<IRouter>() ?? builder.DefaultHandler;
                var route = new ODataRoute( router, versionedRouteName, routePrefix, routeConstraint, inlineConstraintResolver );

                routes.Add( route );
                odataRoutes.Add( route );
            }

            // TODO: do we still need to handle unmatched conventions? how?
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
        public static ODataRoute MapVersionedODataRoute( this IRouteBuilder builder, string routeName, string routePrefix, ApiVersion apiVersion, Action<IContainerBuilder> configureAction )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( routeName, nameof( routeName ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<ODataRoute>() != null );

            IEnumerable<IODataRoutingConvention> NewRoutingConventions( IServiceProvider serviceProvider )
            {
                var model = serviceProvider.GetRequiredService<IEdmModel>();
                var routingConventions = EnsureConventions( ODataRoutingConventions.CreateDefault() );

                model.SetAnnotationValue( model, new ApiVersionAnnotation( apiVersion ) );
                routingConventions.Insert( 0, new VersionedAttributeRoutingConvention( routeName, builder.ServiceProvider, apiVersion ) );

                return routingConventions.ToArray();
            }

            var perRouteContainer = builder.ServiceProvider.GetRequiredService<IPerRouteContainer>();

            if ( !IsNullOrEmpty( routePrefix ) )
            {
                routePrefix = routePrefix.TrimEnd( '/' );
            }

            var options = builder.ServiceProvider.GetRequiredService<ODataOptions>();
            var inlineConstraintResolver = builder.ServiceProvider.GetRequiredService<IInlineConstraintResolver>();
            var unversionedRouteName = routeName + UnversionedRouteSuffix;

            var preConfigureAction = builder.ConfigureDefaultServices(
                container =>
                {
                    container.AddService( Singleton, typeof( IEnumerable<IODataRoutingConvention> ), NewRoutingConventions );
                    configureAction?.Invoke( container );
                } );
            var rootContainer = perRouteContainer.CreateODataRootContainer( routeName, preConfigureAction );
            var pathHandler = rootContainer.GetRequiredService<IODataPathHandler>();
            var router = rootContainer.GetService<IRouter>() ?? builder.DefaultHandler;

            if ( pathHandler != null && pathHandler.UrlKeyDelimiter == null )
            {
                pathHandler.UrlKeyDelimiter = options.UrlKeyDelimiter;
            }

            var routeConstraint = new VersionedODataPathRouteConstraint( routeName, apiVersion );
            var route = new ODataRoute( router, routeName, routePrefix, routeConstraint, inlineConstraintResolver );
            var routes = builder.Routes;

            // TODO: add batching
            // var batchHandler = rootContainer.GetService<ODataBatchHandler>();
            // if ( batchHandler != null )
            // {
            //     batchHandler.ODataRouteName = routeName;
            //     var batchTemplate = IsNullOrEmpty( routePrefix ) ? ODataRouteConstants.Batch : routePrefix + '/' + ODataRouteConstants.Batch;
            //     routes.MapHttpBatchRoute( routeName + nameof( ODataRouteConstants.Batch ), batchTemplate, batchHandler );
            // }
            routes.Add( route );

            // TODO: add unversioned route?
            // var unversionedRouteConstraint = new ODataPathRouteConstraint( routeName );
            // var unversionedRoute = new ODataRoute( router, routeName + UnversionedRouteSuffix, routePrefix, new UnversionedODataPathRouteConstraint( unversionedRouteConstraint, apiVersion ), inlineConstraintResolver );
            // routes.Add( unversionedRoute );
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
            MapVersionedODataRoute( builder, routeName, routePrefix, model, apiVersion, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault(), null );

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
            ODataBatchHandler batchHandler ) =>
            MapVersionedODataRoute( builder, routeName, routePrefix, model, apiVersion, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault(), batchHandler );

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
            MapVersionedODataRoute( builder, routeName, routePrefix, model, apiVersion, pathHandler, routingConventions, null );

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
            ODataBatchHandler batchHandler )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNullOrEmpty( routeName, nameof( routeName ) );
            Arg.NotNull( model, nameof( model ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<ODataRoute>() != null );

            IEnumerable<IODataRoutingConvention> NewRoutingConventions( IServiceProvider serviceProvider )
            {
                var conventions = EnsureConventions( routingConventions.ToList() );
                conventions.Insert( 0, new VersionedAttributeRoutingConvention( routeName, builder.ServiceProvider, apiVersion ) );
                return conventions.ToArray();
            }

            var perRouteContainer = builder.ServiceProvider.GetRequiredService<IPerRouteContainer>();

            if ( !IsNullOrEmpty( routePrefix ) )
            {
                routePrefix = routePrefix.TrimEnd( '/' );
            }

            var options = builder.ServiceProvider.GetRequiredService<ODataOptions>();
            var inlineConstraintResolver = builder.ServiceProvider.GetRequiredService<IInlineConstraintResolver>();
            var unversionedRouteName = routeName + UnversionedRouteSuffix;

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
            var route = new ODataRoute( router, routeName, routePrefix, routeConstraint, inlineConstraintResolver );
            var routes = builder.Routes;

            // TODO: add batching
            // if ( batchHandler != null )
            // {
            //     batchHandler.ODataRouteName = routeName;
            //     var batchTemplate = IsNullOrEmpty( routePrefix ) ? ODataRouteConstants.Batch : routePrefix + '/' + ODataRouteConstants.Batch;
            //     routes.MapHttpBatchRoute( routeName + nameof( ODataRouteConstants.Batch ), batchTemplate, batchHandler );
            // }
            routes.Add( route );

            // TODO: add unversioned route?
            // var unversionedRouteConstraint = new ODataPathRouteConstraint( routeName );
            // var unversionedRoute = new ODataRoute( router, routeName + UnversionedRouteSuffix, routePrefix, new UnversionedODataPathRouteConstraint( unversionedRouteConstraint, apiVersion ), inlineConstraintResolver );
            // routes.Add( unversionedRoute );
            return route;
        }

        static Action<IContainerBuilder> ConfigureDefaultServices( this IRouteBuilder builder, Action<IContainerBuilder> configureAction ) => configureDefaultServicesFunc( builder, configureAction );

        static Func<IRouteBuilder, Action<IContainerBuilder>, Action<IContainerBuilder>> ResolveConfigureDefaultServicesFunc()
        {
            var method = typeof( ODataRouteBuilderExtensions ).GetMethod( "ConfigureDefaultServices", NonPublic | Static, null, new[] { typeof( IRouteBuilder ), typeof( Action<IContainerBuilder> ) }, null );
            return (Func<IRouteBuilder, Action<IContainerBuilder>, Action<IContainerBuilder>>) method.CreateDelegate( typeof( Func<IRouteBuilder, Action<IContainerBuilder>, Action<IContainerBuilder>> ) );
        }

        static IList<IODataRoutingConvention> EnsureConventions( IList<IODataRoutingConvention> conventions )
        {
            Contract.Requires( conventions != null );
            Contract.Ensures( Contract.Result<IList<IODataRoutingConvention>>() != null );

            var discovered = new BitVector32( 0 );

            for ( var i = 0; i < conventions.Count; i++ )
            {
                var convention = conventions[i];

                if ( convention is MetadataRoutingConvention )
                {
                    conventions[i] = new VersionedMetadataRoutingConvention();
                    discovered[1] = true;
                }
                else if ( convention is VersionedMetadataRoutingConvention )
                {
                    discovered[1] = true;
                }
            }

            if ( !discovered[1] )
            {
                conventions.Insert( 0, new VersionedMetadataRoutingConvention() );
            }

            return conventions;
        }

        static IRouteConstraint MakeVersionedODataRouteConstraint( ApiVersion apiVersion, ref string versionedRouteName )
        {
            Contract.Requires( !IsNullOrEmpty( versionedRouteName ) );
            Contract.Ensures( Contract.Result<IRouteConstraint>() != null );

            if ( apiVersion == null )
            {
                return new ODataPathRouteConstraint( versionedRouteName );
            }

            versionedRouteName += "-" + apiVersion.ToString();
            return new VersionedODataPathRouteConstraint( versionedRouteName, apiVersion );
        }
    }
}