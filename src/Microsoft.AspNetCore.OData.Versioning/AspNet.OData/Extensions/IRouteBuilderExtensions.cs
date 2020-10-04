namespace Microsoft.AspNet.OData.Extensions
{
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using static Microsoft.OData.ServiceLifetime;

    /// <summary>
    /// Provides extension methods for registering versioned OData routes.
    /// </summary>
    [CLSCompliant( false )]
    public static class IRouteBuilderExtensions
    {
        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="modelBuilder">The <see cref="VersionedODataModelBuilder">model builer</see> used to create
        /// an <see cref="IEdmModel">EDM model</see> per API version.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapVersionedODataRoute(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            VersionedODataModelBuilder modelBuilder )
        {
            if ( modelBuilder == null )
            {
                throw new ArgumentNullException( nameof( modelBuilder ) );
            }

            return builder.MapVersionedODataRoute( routeName, routePrefix, modelBuilder.GetEdmModels( routePrefix ) );
        }

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="modelBuilder">The <see cref="VersionedODataModelBuilder">model builer</see> used to create
        /// an <see cref="IEdmModel">EDM model</see> per API version.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapVersionedODataRoute(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            VersionedODataModelBuilder modelBuilder,
            Action<IContainerBuilder> configureAction )
        {
            if ( modelBuilder == null )
            {
                throw new ArgumentNullException( nameof( modelBuilder ) );
            }

            return builder.MapVersionedODataRoute( routeName, routePrefix, modelBuilder.GetEdmModels( routePrefix ), configureAction );
        }

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapVersionedODataRoute(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            Action<IContainerBuilder> configureAction ) =>
            AddRoute(
                builder,
                builder.MapODataServiceRoute(
                    routeName,
                    routePrefix,
                    container =>
                    {
                        container.AddApiVersioning( routeName, models, builder.ServiceProvider );
                        configureAction?.Invoke( container );
                    } ) );

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapVersionedODataRoute(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models ) =>
            AddRoute(
                builder,
                builder.MapODataServiceRoute(
                    routeName,
                    routePrefix,
                    container => container.AddApiVersioning( routeName, models, builder.ServiceProvider ) ) );

        /// <summary>
        /// Maps the specified OData route and the OData route attributes. When the <paramref name="batchHandler"/> is
        /// non-<c>null</c>, it will create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler"/>.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapVersionedODataRoute(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            ODataBatchHandler batchHandler ) =>
            AddRoute(
                builder,
                builder.MapODataServiceRoute(
                    routeName,
                    routePrefix,
                    container => container.AddApiVersioning( routeName, models, builder.ServiceProvider )
                                          .AddService( Singleton, sp => batchHandler ) ) );

        /// <summary>
        /// Maps the specified OData route.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler"/> to use for parsing the OData path.</param>
        /// <param name="routingConventions">The OData routing conventions to use for controller and action selection.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapVersionedODataRoute(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions ) =>
            AddRoute(
                builder,
                builder.MapODataServiceRoute(
                    routeName,
                    routePrefix,
                    container =>
                        container.AddApiVersioning( models, routingConventions, builder.ServiceProvider )
                                 .AddService( Singleton, sp => pathHandler ) ) );

        /// <summary>
        /// Maps the specified OData route. When the <paramref name="batchHandler"/> is non-<c>null</c>, it will
        /// create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler" /> to use for parsing the OData path.</param>
        /// <param name="routingConventions">The OData routing conventions to use for controller and action selection.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler"/>.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapVersionedODataRoute(
            this IRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions,
            ODataBatchHandler batchHandler ) =>
            AddRoute(
                builder,
                builder.MapODataServiceRoute(
                    routeName,
                    routePrefix,
                    container =>
                        container.AddApiVersioning( models, routingConventions, builder.ServiceProvider )
                                 .AddService( Singleton, sp => pathHandler )
                                 .AddService( Singleton, sp => batchHandler ) ) );

        static ODataRoute AddRoute( IRouteBuilder builder, ODataRoute route )
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            var serviceProvider = builder.ServiceProvider;
            var routeCollection = serviceProvider.GetRequiredService<IODataRouteCollectionProvider>();
            var container = serviceProvider.GetRequiredService<IPerRouteContainer>();

            routeCollection.Add( new ODataRouteMapping( route.Name, route.RoutePrefix, serviceProvider ) );
            container.AddRoute( route.Name, route.RoutePrefix );

            return route;
        }
    }
}