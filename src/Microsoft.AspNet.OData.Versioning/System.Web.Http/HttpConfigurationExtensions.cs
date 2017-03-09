namespace System.Web.Http
{
    using Collections.Generic;
    using Collections.Specialized;
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Linq;
    using Microsoft;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Routing;
    using Microsoft.Web.OData.Builder;
    using Microsoft.Web.OData.Routing;
    using OData.Batch;
    using OData.Extensions;
    using OData.Routing;
    using OData.Routing.Conventions;
    using Routing;
    using static Linq.Expressions.Expression;
    using static System.String;
    using static System.StringComparison;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        const string ResolverSettingsKey = "System.Web.OData.ResolverSettingsKey";
        const string UnversionedRouteSuffix = "-Unversioned";
        const string ApiVersionConstraintName = "apiVersion";
        const string ApiVersionConstraint = "{" + ApiVersionConstraintName + "}";
        static readonly Lazy<Action<DefaultODataPathHandler, object>> setResolverSettings = new Lazy<Action<DefaultODataPathHandler, object>>( GetResolverSettingsMutator );

        static Action<DefaultODataPathHandler, object> GetResolverSettingsMutator()
        {
            Contract.Ensures( Contract.Result<Action<DefaultODataPathHandler, object>>() != null );

            // build a strong-typed delegate to the DefaultODataPathHandler.ResolverSettings property mutator
            var handlerType = typeof( DefaultODataPathHandler );
            var resolverSettingsType = handlerType.Assembly.GetType( "System.Web.OData.ODataUriResolverSetttings" );
            var h = Parameter( handlerType, "h" );
            var rs = Parameter( typeof( object ), "rs" );
            var property = Property( h, "ResolverSetttings" );
            var body = Assign( property, Convert( rs, resolverSettingsType ) );
            var lambda = Lambda<Action<DefaultODataPathHandler, object>>( body, h, rs );
            var action = lambda.Compile();

            return action;
        }

        static void SetResolverSettings( this HttpConfiguration configuration, IODataPathHandler pathHandler )
        {
            Contract.Requires( configuration != null );

            // REMARKS: the DefaultODataPathHandler.ResolverSettings property is internal as is the ODataUriResolverSetttings class.
            // The MapODataServiceRoute normally hooks this up, but we are replacing that process. in order to retain functional
            // fidelity we'll build and compile a strong-typed delegate that can be used to set the property.
            //
            // in additional, the ODataUriResolverSetttings are created lazy-initialized from the property bag. instead of using
            // Reflection, we'll test for the known key. if the key is not present, we'll use a public extension method
            // (e.g. EnableCaseInsensitive) with the default, unconfigured value. this will trigger the creation of the
            // settings and populate the property.

            var handler = pathHandler as DefaultODataPathHandler;

            if ( handler == null )
            {
                return;
            }

            // REMARKS: this creates and populates the ODataUriResolverSetttings; OData URLs are case-sensitive by default.
            if ( !configuration.Properties.ContainsKey( ResolverSettingsKey ) )
            {
                configuration.EnableCaseInsensitive( false );
            }

            setResolverSettings.Value( handler, configuration.Properties[ResolverSettingsKey] );
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
            MapVersionedODataRoutes( configuration, routeName, routePrefix, models, null );

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
            ODataBatchHandler batchHandler ) =>
            MapVersionedODataRoutes( configuration, routeName, routePrefix, models, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault(), batchHandler );

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
            MapVersionedODataRoutes( configuration, routeName, routePrefix, models, pathHandler, routingConventions, null );

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
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "The specified handler must be the batch handler." )]
        public static IReadOnlyList<ODataRoute> MapVersionedODataRoutes(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions,
            ODataBatchHandler batchHandler )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( models, nameof( models ) );
            Contract.Ensures( Contract.Result<IReadOnlyList<ODataRoute>>() != null );

            var routeConventions = EnsureConventions( routingConventions.ToList() );
            var routes = configuration.Routes;

            if ( !IsNullOrEmpty( routePrefix ) )
            {
                routePrefix = routePrefix.TrimEnd( '/' );
            }

            if ( batchHandler != null )
            {
                var batchTemplate = IsNullOrEmpty( routePrefix ) ? ODataRouteConstants.Batch : routePrefix + '/' + ODataRouteConstants.Batch;
                routes.MapHttpBatchRoute( routeName + "Batch", batchTemplate, batchHandler );
            }

            configuration.SetResolverSettings( pathHandler );
            routeConventions.Insert( 0, null );

            var odataRoutes = new List<ODataRoute>();
            var unversionedConstraints = new List<IHttpRouteConstraint>();

            foreach ( var model in models )
            {
                var versionedRouteName = routeName;
                var routeConstraint = default( ODataPathRouteConstraint );

                routeConventions[0] = new VersionedAttributeRoutingConvention( model, configuration );
                routeConstraint = new ODataPathRouteConstraint( pathHandler, model, versionedRouteName, routeConventions.ToArray() );
                unversionedConstraints.Add( routeConstraint );
                routeConstraint = MakeVersionedODataRouteConstraint( routeConstraint, pathHandler, routeConventions, model, ref versionedRouteName );

                var route = new ODataRoute( routePrefix, routeConstraint );

                AddApiVersionConstraintIfNecessary( route );
                routes.Add( versionedRouteName, route );
                odataRoutes.Add( route );
            }

            AddRouteToRespondWithBadRequestWhenAtLeastOneRouteCouldMatch( routeName, routePrefix, routes, odataRoutes, unversionedConstraints );

            return odataRoutes;
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
            MapVersionedODataRoute( configuration, routeName, routePrefix, model, apiVersion, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault(), null );

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
            ODataBatchHandler batchHandler ) =>
            MapVersionedODataRoute( configuration, routeName, routePrefix, model, apiVersion, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault(), batchHandler );

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
            MapVersionedODataRoute( configuration, routeName, routePrefix, model, apiVersion, pathHandler, routingConventions, null );

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
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "The specified handler must be the batch handler." )]
        public static ODataRoute MapVersionedODataRoute(
            this HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            IEdmModel model,
            ApiVersion apiVersion,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions,
            ODataBatchHandler batchHandler )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( model, nameof( model ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<ODataRoute>() != null );

            var routeConventions = EnsureConventions( routingConventions.ToList() );
            var routes = configuration.Routes;

            if ( !IsNullOrEmpty( routePrefix ) )
            {
                routePrefix = routePrefix.TrimEnd( '/' );
            }

            if ( batchHandler != null )
            {
                var batchTemplate = IsNullOrEmpty( routePrefix ) ? ODataRouteConstants.Batch : routePrefix + '/' + ODataRouteConstants.Batch;
                routes.MapHttpBatchRoute( routeName + "Batch", batchTemplate, batchHandler );
            }

            configuration.SetResolverSettings( pathHandler );
            model.SetAnnotationValue( model, new ApiVersionAnnotation( apiVersion ) );
            routeConventions.Insert( 0, new VersionedAttributeRoutingConvention( model, configuration ) );

            var routeConstraint = new VersionedODataPathRouteConstraint( pathHandler, model, routeName, routeConventions.ToArray(), apiVersion );
            var route = new ODataRoute( routePrefix, routeConstraint );

            AddApiVersionConstraintIfNecessary( route );
            routes.Add( routeName, route );

            var unversionedRouteConstraint = new ODataPathRouteConstraint( pathHandler, model, routeName, routeConventions.ToArray() );
            var unversionedRoute = new ODataRoute( routePrefix, new UnversionedODataPathRouteConstraint( unversionedRouteConstraint, apiVersion ) );

            AddApiVersionConstraintIfNecessary( unversionedRoute );
            routes.Add( routeName + UnversionedRouteSuffix, unversionedRoute );

            return route;
        }

        static ODataPathRouteConstraint MakeVersionedODataRouteConstraint(
            ODataPathRouteConstraint routeConstraint,
            IODataPathHandler pathHandler,
            IList<IODataRoutingConvention> routeConventions,
            IEdmModel model,
            ref string versionedRouteName )
        {
            Contract.Requires( routeConstraint != null );
            Contract.Requires( pathHandler != null );
            Contract.Requires( routeConventions != null );
            Contract.Requires( model != null );
            Contract.Requires( !IsNullOrEmpty( versionedRouteName ) );
            Contract.Ensures( Contract.Result<ODataPathRouteConstraint>() != null );

            var apiVersion = model.GetAnnotationValue<ApiVersionAnnotation>( model )?.ApiVersion;

            if ( apiVersion == null )
            {
                return routeConstraint;
            }

            versionedRouteName += "-" + apiVersion.ToString();
            return new VersionedODataPathRouteConstraint( pathHandler, model, versionedRouteName, routeConventions.ToArray(), apiVersion );
        }

        static void AddApiVersionConstraintIfNecessary( ODataRoute route )
        {
            Contract.Requires( route != null );

            var routePrefix = route.RoutePrefix;

            if ( routePrefix == null || routePrefix.IndexOf( ApiVersionConstraint, Ordinal ) < 0 || route.Constraints.ContainsKey( ApiVersionConstraintName ) )
            {
                return;
            }

            // note: even though the constraints are a dictionary, it's important to rebuild the entire collection
            // to make sure the api version constraint is evaluated first; otherwise, the current api version will
            // not be resolved when the odata versioning constraint is evaluated
            var originalConstraints = new Dictionary<string, object>( route.Constraints );

            route.Constraints.Clear();
            route.Constraints.Add( ApiVersionConstraintName, new ApiVersionRouteConstraint() );

            foreach ( var constraint in originalConstraints )
            {
                route.Constraints.Add( constraint.Key, constraint.Value );
            }
        }

        static void AddRouteToRespondWithBadRequestWhenAtLeastOneRouteCouldMatch( string routeName, string routePrefix, HttpRouteCollection routes, List<ODataRoute> odataRoutes, List<IHttpRouteConstraint> unversionedConstraints )
        {
            Contract.Requires( !IsNullOrEmpty( routeName ) );
            Contract.Requires( routes != null );
            Contract.Requires( odataRoutes != null );
            Contract.Requires( unversionedConstraints != null );

            var unversionedRoute = new ODataRoute( routePrefix, new UnversionedODataPathRouteConstraint( unversionedConstraints ) );

            AddApiVersionConstraintIfNecessary( unversionedRoute );
            routes.Add( routeName + UnversionedRouteSuffix, unversionedRoute );
            odataRoutes.Add( unversionedRoute );
        }
    }
}