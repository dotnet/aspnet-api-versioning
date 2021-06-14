namespace Microsoft.AspNet.OData.Extensions
{
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using static Microsoft.OData.ServiceLifetime;

    /// <summary>
    /// Provides extension methods for registering versioned OData route endpoints.
    /// </summary>
    [CLSCompliant( false )]
    public static class IEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="modelBuilder">The <see cref="VersionedODataModelBuilder">model builer</see> used to create
        /// an <see cref="IEdmModel">EDM model</see> per API version.</param>
        /// <returns>The input <see cref="IEndpointRouteBuilder"/>.</returns>
        public static IEndpointRouteBuilder MapVersionedODataRoute(
            this IEndpointRouteBuilder builder,
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
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="modelBuilder">The <see cref="VersionedODataModelBuilder">model builer</see> used to create
        /// an <see cref="IEdmModel">EDM model</see> per API version.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The input <see cref="IEndpointRouteBuilder"/>.</returns>
        public static IEndpointRouteBuilder MapVersionedODataRoute(
            this IEndpointRouteBuilder builder,
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
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <returns>The input <see cref="IEndpointRouteBuilder"/>.</returns>
        public static IEndpointRouteBuilder MapVersionedODataRoute(
            this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models ) =>
            AddRoute(
                builder.MapODataRoute(
                    routeName,
                    routePrefix,
                    container => container.AddApiVersioning( routeName, models, builder.ServiceProvider ) ),
                routeName,
                routePrefix );

        /// <summary>
        /// Maps the specified OData route and the OData route attributes. When the <paramref name="batchHandler"/> is
        /// non-<c>null</c>, it will create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler"/>.</param>
        /// <returns>The <see cref="IEndpointRouteBuilder"/>.</returns>
        public static IEndpointRouteBuilder MapVersionedODataRoute(
            this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            ODataBatchHandler batchHandler ) =>
            AddRoute(
                builder.MapODataRoute(
                    routeName,
                    routePrefix,
                    container => container.AddApiVersioning( routeName, models, builder.ServiceProvider )
                                          .AddService( Singleton, sp => batchHandler ) ),
                routeName,
                routePrefix );

        /// <summary>
        /// Maps the specified OData route.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler"/> to use for parsing the OData path.</param>
        /// <param name="routingConventions">
        /// The OData routing conventions to use for controller and action selection.
        /// </param>
        /// <returns>The <see cref="IEndpointRouteBuilder"/>.</returns>
        public static IEndpointRouteBuilder MapVersionedODataRoute(
            this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions ) =>
            AddRoute(
                builder.MapODataRoute(
                    routeName,
                    routePrefix,
                    container => container.AddApiVersioning( models, routingConventions, builder.ServiceProvider )
                                          .AddService( Singleton, sp => pathHandler ) ),
                routeName,
                routePrefix );

        /// <summary>
        /// Maps the specified OData route. When the <paramref name="batchHandler"/> is non-<c>null</c>, it will
        /// create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler" /> to use for parsing the OData path.</param>
        /// <param name="routingConventions">
        /// The OData routing conventions to use for controller and action selection.
        /// </param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler"/>.</param>
        /// <returns>The <see cref="IEndpointRouteBuilder"/>.</returns>
        public static IEndpointRouteBuilder MapVersionedODataRoute(
            this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions,
            ODataBatchHandler batchHandler ) =>
            AddRoute(
                builder.MapODataRoute(
                    routeName,
                    routePrefix,
                    container =>
                        container.AddApiVersioning( models, routingConventions, builder.ServiceProvider )
                                 .AddService( Singleton, sp => pathHandler )
                                 .AddService( Singleton, sp => batchHandler ) ),
                routeName,
                routePrefix );

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The input <see cref="IEndpointRouteBuilder"/>.</returns>
        public static IEndpointRouteBuilder MapVersionedODataRoute(
            this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            Action<IContainerBuilder> configureAction ) =>
            AddRoute(
                builder.MapODataRoute(
                    routeName,
                    routePrefix,
                    container =>
                    {
                        container.AddApiVersioning( routeName, models, builder.ServiceProvider );
                        configureAction?.Invoke( container );
                    } ),
                routeName,
                routePrefix );

        static IEndpointRouteBuilder AddRoute( IEndpointRouteBuilder builder, string routeName, string routePrefix )
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            var serviceProvider = builder.ServiceProvider;
            var routeCollection = serviceProvider.GetRequiredService<IODataRouteCollectionProvider>();

            routeCollection.Add( new ODataRouteMapping( routeName, routePrefix, serviceProvider ) );

            return builder;
        }
    }
}