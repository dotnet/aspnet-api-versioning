namespace Microsoft.Web.Http.Dispatcher
{
    using Controllers;
    using Routing;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using Versioning;
    using static Controllers.HttpControllerDescriptorComparer;
    using static System.StringComparer;
    using static Versioning.ErrorCodes;

    /// <summary>
    /// Represents the logic for selecting a versioned controller.
    /// </summary>
    public class ApiVersionControllerSelector : IHttpControllerSelector
    {
        readonly HttpConfiguration configuration;
        readonly ApiVersioningOptions options;
        readonly HttpControllerTypeCache controllerTypeCache;
        readonly Lazy<ConcurrentDictionary<string, HttpControllerDescriptorGroup>> controllerInfoCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionControllerSelector"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> to initialize
        /// the controller selector with.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">service versioning options</see>
        /// associated with the controller selector.</param>
        public ApiVersionControllerSelector( HttpConfiguration configuration, ApiVersioningOptions options )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( options, nameof( options ) );

            this.configuration = configuration;
            this.options = options;
            controllerInfoCache = new Lazy<ConcurrentDictionary<string, HttpControllerDescriptorGroup>>( InitializeControllerInfoCache );
            controllerTypeCache = new HttpControllerTypeCache( this.configuration );
        }

        /// <summary>
        /// Creates and returns a controller descriptor mapping.
        /// </summary>
        /// <returns>A <see cref="IDictionary{TKey,TValue}">collection</see> of route-to-controller mapping.</returns>
        public virtual IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            Contract.Ensures( Contract.Result<IDictionary<string, HttpControllerDescriptor>>() != null );

            var mapping = from pair in controllerInfoCache.Value
                          where pair.Value.Count > 0
                          select pair;

            return mapping.ToDictionary( p => p.Key, p => (HttpControllerDescriptor) p.Value, OrdinalIgnoreCase );
        }

        /// <summary>
        /// Selects and returns the controller descriptor to invoke given the provided request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">request</see> to get a controller descriptor for.</param>
        /// <returns>The <see cref="HttpControllerDescriptor">controller descriptor</see> that matches the specified <paramref name="request"/>.</returns>
        public virtual HttpControllerDescriptor SelectController( HttpRequestMessage request )
        {
            Arg.NotNull( request, nameof( request ) );
            Contract.Ensures( Contract.Result<HttpControllerDescriptor>() != null );

            EnsureRequestHasValidApiVersion( request );

            var aggregator = new ApiVersionControllerAggregator( request, GetControllerName, controllerInfoCache );
            var conventionRouteSelector = new ConventionRouteControllerSelector( options, controllerTypeCache );
            var conventionRouteResult = default( ControllerSelectionResult );
            var exceptionFactory = new HttpResponseExceptionFactory( request, new Lazy<ApiVersionModel>( () => aggregator.AllVersions ) );

            if ( aggregator.RouteData == null )
            {
                conventionRouteResult = conventionRouteSelector.SelectController( aggregator );

                if ( conventionRouteResult.Succeeded )
                {
                    return conventionRouteResult.Controller;
                }

                throw exceptionFactory.NewNotFoundOrBadRequestException( conventionRouteResult, null );
            }

            var directRouteSelector = new DirectRouteControllerSelector( options );
            var directRouteResult = directRouteSelector.SelectController( aggregator );

            if ( directRouteResult.Succeeded )
            {
                return directRouteResult.Controller;
            }

            conventionRouteResult = conventionRouteSelector.SelectController( aggregator );

            if ( conventionRouteResult.Succeeded )
            {
                return conventionRouteResult.Controller;
            }

            throw exceptionFactory.NewNotFoundOrBadRequestException( conventionRouteResult, directRouteResult );
        }

        /// <summary>
        /// Gets the name of the controller for the specified request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">request</see> to the controller name for.</param>
        /// <returns>The name of the controller for the specified <paramref name="request"/>.</returns>
        public virtual string GetControllerName( HttpRequestMessage request )
        {
            Arg.NotNull( request, nameof( request ) );

            var routeData = request.GetRouteData();

            if ( routeData == null )
            {
                return null;
            }

            routeData.Values.TryGetValue( RouteDataTokenKeys.Controller, out string controller );

            return controller;
        }

        ConcurrentDictionary<string, HttpControllerDescriptorGroup> InitializeControllerInfoCache()
        {
            var mapping = new ConcurrentDictionary<string, HttpControllerDescriptorGroup>( OrdinalIgnoreCase );

            foreach ( var pair in controllerTypeCache.Cache )
            {
                var key = pair.Key;
                var descriptors = new List<HttpControllerDescriptor>();

                foreach ( var grouping in pair.Value )
                {
                    foreach ( var type in grouping )
                    {
                        descriptors.Add( new HttpControllerDescriptor( configuration, key, type ) );
                    }
                }

                descriptors.Sort( ByVersion );

                var descriptorGroup = new HttpControllerDescriptorGroup( configuration, key, descriptors.ToArray() );

                mapping.TryAdd( key, descriptorGroup );
            }

            return mapping;
        }

        static void EnsureRequestHasValidApiVersion( HttpRequestMessage request )
        {
            Contract.Requires( request != null );

            try
            {
                var apiVersion = request.GetRequestedApiVersion();
            }
            catch ( AmbiguousApiVersionException ex )
            {
                var options = request.GetApiVersioningOptions();
                throw new HttpResponseException( options.ErrorResponses.BadRequest( request, AmbiguousApiVersion, ex.Message ) );
            }
        }
    }
}