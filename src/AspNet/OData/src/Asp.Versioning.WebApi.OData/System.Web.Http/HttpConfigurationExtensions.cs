// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using Asp.Versioning.OData;
using Asp.Versioning.Routing;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using System.Net.Http;
using static Microsoft.OData.ServiceLifetime;

/// <summary>
/// Provides extension methods for the <see cref="HttpConfiguration"/> class.
/// </summary>
public static class HttpConfigurationExtensions
{
    /// <summary>
    /// Maps the specified OData route and the OData route attributes.
    /// </summary>
    /// <param name="configuration">The server configuration.</param>
    /// <param name="routeName">The name of the route to map.</param>
    /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
    /// <param name="modelBuilder">The <see cref="VersionedODataModelBuilder">model builer</see> used to create
    /// an <see cref="IEdmModel">EDM model</see> per API version.</param>
    /// <returns>The added <see cref="ODataRoute"/>.</returns>
    public static ODataRoute MapVersionedODataRoute(
        this HttpConfiguration configuration,
        string routeName,
        string routePrefix,
        VersionedODataModelBuilder modelBuilder )
    {
        if ( modelBuilder == null )
        {
            throw new ArgumentNullException( nameof( modelBuilder ) );
        }

        return configuration.MapVersionedODataRoute( routeName, routePrefix, modelBuilder.GetEdmModels( routePrefix ) );
    }

    /// <summary>
    /// Maps the specified OData route and the OData route attributes.
    /// </summary>
    /// <param name="configuration">The server configuration.</param>
    /// <param name="routeName">The name of the route to map.</param>
    /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
    /// <param name="modelBuilder">The <see cref="VersionedODataModelBuilder">model builer</see> used to create
    /// an <see cref="IEdmModel">EDM model</see> per API version.</param>
    /// <param name="configureAction">The configuring action to add the services to the root container.</param>
    /// <returns>The added <see cref="ODataRoute"/>.</returns>
    public static ODataRoute MapVersionedODataRoute(
        this HttpConfiguration configuration,
        string routeName,
        string routePrefix,
        VersionedODataModelBuilder modelBuilder,
        Action<IContainerBuilder> configureAction )
    {
        if ( modelBuilder == null )
        {
            throw new ArgumentNullException( nameof( modelBuilder ) );
        }

        return configuration.MapVersionedODataRoute( routeName, routePrefix, modelBuilder.GetEdmModels( routePrefix ), configureAction );
    }

    /// <summary>
    /// Maps the specified OData route and the OData route attributes.
    /// </summary>
    /// <param name="configuration">The server configuration.</param>
    /// <param name="routeName">The name of the route to map.</param>
    /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
    /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
    /// <param name="configureAction">The configuring action to add the services to the root container.</param>
    /// <returns>The added <see cref="ODataRoute"/>.</returns>
    public static ODataRoute MapVersionedODataRoute(
        this HttpConfiguration configuration,
        string routeName,
        string routePrefix,
        IEnumerable<IEdmModel> models,
        Action<IContainerBuilder> configureAction ) =>
        AddApiVersionConstraintIfNecessary(
            configuration,
            configuration.MapODataServiceRoute(
                routeName,
                routePrefix,
                builder =>
                {
                    builder.AddApiVersioning( routeName, models );
                    configureAction?.Invoke( builder );
                } ) );

    /// <summary>
    /// Maps the specified OData route and the OData route attributes.
    /// </summary>
    /// <param name="configuration">The server configuration.</param>
    /// <param name="routeName">The name of the route to map.</param>
    /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
    /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
    /// <returns>The added <see cref="ODataRoute"/>.</returns>
    public static ODataRoute MapVersionedODataRoute(
        this HttpConfiguration configuration,
        string routeName,
        string routePrefix,
        IEnumerable<IEdmModel> models ) =>
        AddApiVersionConstraintIfNecessary(
            configuration,
            configuration.MapODataServiceRoute( routeName, routePrefix, builder => builder.AddApiVersioning( routeName, models ) ) );

    /// <summary>
    /// Maps the specified OData route and the OData route attributes. When the <paramref name="batchHandler"/> is
    /// non-<c>null</c>, it will create a '$batch' endpoint to handle the batch requests.
    /// </summary>
    /// <param name="configuration">The server configuration.</param>
    /// <param name="routeName">The name of the route to map.</param>
    /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
    /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
    /// <param name="batchHandler">The <see cref="ODataBatchHandler"/>.</param>
    /// <returns>The added <see cref="ODataRoute"/>.</returns>
    public static ODataRoute MapVersionedODataRoute(
        this HttpConfiguration configuration,
        string routeName,
        string routePrefix,
        IEnumerable<IEdmModel> models,
        ODataBatchHandler batchHandler ) =>
        AddApiVersionConstraintIfNecessary(
            configuration,
            configuration.MapODataServiceRoute(
                routeName,
                routePrefix,
                builder => builder.AddApiVersioning( routeName, models )
                                  .AddService( Singleton, sp => batchHandler ) ) );

    /// <summary>
    /// Maps the specified OData route and the OData route attributes. When the <paramref name="defaultHandler"/>
    /// is non-<c>null</c>, it will map it as the default handler for the route.
    /// </summary>
    /// <param name="configuration">The server configuration.</param>
    /// <param name="routeName">The name of the route to map.</param>
    /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
    /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
    /// <param name="defaultHandler">The default <see cref="HttpMessageHandler"/> for this route.</param>
    /// <returns>The added <see cref="ODataRoute"/>.</returns>
    public static ODataRoute MapVersionedODataRoute(
        this HttpConfiguration configuration,
        string routeName,
        string routePrefix,
        IEnumerable<IEdmModel> models,
        HttpMessageHandler defaultHandler ) =>
        AddApiVersionConstraintIfNecessary(
            configuration,
            configuration.MapODataServiceRoute(
                routeName,
                routePrefix,
                builder => builder.AddApiVersioning( routeName, models )
                                  .AddService( Singleton, sp => defaultHandler ) ) );

    /// <summary>
    /// Maps the specified OData route.
    /// </summary>
    /// <param name="configuration">The server configuration.</param>
    /// <param name="routeName">The name of the route to map.</param>
    /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
    /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
    /// <param name="pathHandler">The <see cref="IODataPathHandler"/> to use for parsing the OData path.</param>
    /// <param name="routingConventions">The OData routing conventions to use for controller and action selection.</param>
    /// <returns>The added <see cref="ODataRoute"/>.</returns>
    public static ODataRoute MapVersionedODataRoute(
        this HttpConfiguration configuration,
        string routeName,
        string routePrefix,
        IEnumerable<IEdmModel> models,
        IODataPathHandler pathHandler,
        IEnumerable<IODataRoutingConvention> routingConventions ) =>
        AddApiVersionConstraintIfNecessary(
            configuration,
            configuration.MapODataServiceRoute(
                routeName,
                routePrefix,
                builder => builder.AddApiVersioning( models, routingConventions )
                                  .AddService( Singleton, sp => pathHandler ) ) );

    /// <summary>
    /// Maps the specified OData route. When the <paramref name="batchHandler"/> is non-<c>null</c>, it will
    /// create a '$batch' endpoint to handle the batch requests.
    /// </summary>
    /// <param name="configuration">The server configuration.</param>
    /// <param name="routeName">The name of the route to map.</param>
    /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
    /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
    /// <param name="pathHandler">The <see cref="IODataPathHandler" /> to use for parsing the OData path.</param>
    /// <param name="routingConventions">The OData routing conventions to use for controller and action selection.</param>
    /// <param name="batchHandler">The <see cref="ODataBatchHandler"/>.</param>
    /// <returns>The added <see cref="ODataRoute"/>.</returns>
    public static ODataRoute MapVersionedODataRoute(
        this HttpConfiguration configuration,
        string routeName,
        string routePrefix,
        IEnumerable<IEdmModel> models,
        IODataPathHandler pathHandler,
        IEnumerable<IODataRoutingConvention> routingConventions,
        ODataBatchHandler batchHandler ) =>
        AddApiVersionConstraintIfNecessary(
            configuration,
            configuration.MapODataServiceRoute(
                routeName,
                routePrefix,
                builder =>
                    builder.AddApiVersioning( models, routingConventions )
                           .AddService( Singleton, sp => pathHandler )
                           .AddService( Singleton, sp => batchHandler ) ) );

    /// <summary>
    /// Maps the specified OData route. When the <paramref name="defaultHandler"/> is non-<c>null</c>, it will map
    /// it as the handler for the route.
    /// </summary>
    /// <param name="configuration">The server configuration.</param>
    /// <param name="routeName">The name of the route to map.</param>
    /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
    /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
    /// <param name="pathHandler">The <see cref="IODataPathHandler" /> to use for parsing the OData path.</param>
    /// <param name="routingConventions">The OData routing conventions to use for controller and action selection.</param>
    /// <param name="defaultHandler">The default <see cref="HttpMessageHandler"/> for this route.</param>
    /// <returns>The added <see cref="ODataRoute"/>.</returns>
    public static ODataRoute MapVersionedODataRoute(
        this HttpConfiguration configuration,
        string routeName,
        string routePrefix,
        IEnumerable<IEdmModel> models,
        IODataPathHandler pathHandler,
        IEnumerable<IODataRoutingConvention> routingConventions,
        HttpMessageHandler defaultHandler ) =>
        AddApiVersionConstraintIfNecessary(
            configuration,
            configuration.MapODataServiceRoute(
                routeName,
                routePrefix,
                builder =>
                    builder.AddApiVersioning( models, routingConventions )
                           .AddService( Singleton, sp => pathHandler )
                           .AddService( Singleton, sp => defaultHandler ) ) );

    internal static ODataUrlKeyDelimiter? GetUrlKeyDelimiter( this HttpConfiguration configuration )
    {
        const string UrlKeyDelimiterKey = "Microsoft.AspNet.OData.UrlKeyDelimiterKey";

        if ( configuration.Properties.TryGetValue( UrlKeyDelimiterKey, out var value ) )
        {
            return value as ODataUrlKeyDelimiter;
        }

        configuration.Properties[UrlKeyDelimiterKey] = null;
        return null;
    }

    private static ODataRoute AddApiVersionConstraintIfNecessary( HttpConfiguration configuration, ODataRoute route )
    {
        if ( configuration == null )
        {
            throw new ArgumentNullException( nameof( configuration ) );
        }

        var routePrefix = route.RoutePrefix;

        if ( string.IsNullOrEmpty( routePrefix ) )
        {
            return route;
        }

        var options = configuration.GetApiVersioningOptions();

        if ( route.Constraints.ContainsKey( options.RouteConstraintName ) )
        {
            return route;
        }

        var apiVersionConstraint = "{" + options.RouteConstraintName + "}";
        var absent = routePrefix.IndexOf( apiVersionConstraint, StringComparison.Ordinal ) < 0;

        if ( absent )
        {
            return route;
        }

        // note: even though the constraints are a dictionary, it's important to rebuild the entire collection
        // to make sure the api version constraint is evaluated first; otherwise, the current api version will
        // not be resolved when the odata versioning constraint is evaluated
        var constraints = route.Constraints.ToArray();

        route.Constraints.Clear();
        route.Constraints.Add( options.RouteConstraintName, new ApiVersionRouteConstraint() );

        for ( var i = 0; i < constraints.Length; i++ )
        {
            route.Constraints.Add( constraints[i].Key, constraints[i].Value );
        }

        return route;
    }
}