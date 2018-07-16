namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.OData;
    using System;
    using System.Diagnostics.Contracts;
    using static Microsoft.AspNetCore.Routing.RouteDirection;

    /// <summary>
    /// Represents an <see cref="ODataPathRouteConstraint">OData path route constraint</see> which supports versioning.
    /// </summary>
    [CLSCompliant( false )]
    public class VersionedODataPathRouteConstraint : ODataPathRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedODataPathRouteConstraint" /> class.
        /// </summary>
        /// <param name="routeName">The name of the route this constraint is associated with.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the route constraint.</param>
        public VersionedODataPathRouteConstraint( string routeName, ApiVersion apiVersion )
            : base( routeName )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            ApiVersion = apiVersion;
        }

        /// <summary>
        /// Gets the API version matched by the current OData path route constraint.
        /// </summary>
        /// <value>The <see cref="ApiVersion">API version</see> associated with the route constraint.</value>
        public ApiVersion ApiVersion { get; }

        /// <summary>
        /// Determines whether the route constraint matches the specified criteria.
        /// </summary>
        /// <param name="httpContext">The current <see cref="HttpContext">HTTP context</see>.</param>
        /// <param name="route">The current <see cref="IRouter">route</see>.</param>
        /// <param name="routeKey">The key of the route parameter to match.</param>
        /// <param name="values">The current <see cref="RouteValueDictionary">collection</see> of route values.</param>
        /// <param name="routeDirection">The <see cref="RouteDirection">route direction</see> to match.</param>
        /// <returns>True if the route constraint is matched; otherwise, false.</returns>
        public override bool Match( HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection )
        {
            Arg.NotNull( httpContext, nameof( httpContext ) );
            Arg.NotNull( route, nameof( route ) );
            Arg.NotNull( values, nameof( values ) );

            if ( routeDirection == UrlGeneration )
            {
                return base.Match( httpContext, route, routeKey, values, routeDirection );
            }

            var request = httpContext.Request;
            var feature = httpContext.Features.Get<IApiVersioningFeature>();

            if ( !TryGetRequestedApiVersion( httpContext, feature, out var requestedVersion ) )
            {
                // note: if an error occurs reading the api version, still let the base constraint
                // match the request. the IActionSelector will produce 400 during action selection.
                return base.Match( httpContext, route, routeKey, values, routeDirection );
            }

            if ( requestedVersion != null )
            {
                return ApiVersion == requestedVersion && base.Match( httpContext, route, routeKey, values, routeDirection );
            }

            var options = httpContext.RequestServices.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;

            if ( options.DefaultApiVersion != ApiVersion || !base.Match( httpContext, route, routeKey, values, routeDirection ) )
            {
                return false;
            }

            if ( options.AssumeDefaultVersionWhenUnspecified || IsServiceDocumentOrMetadataRoute( values ) )
            {
                feature.RequestedApiVersion = ApiVersion;
            }

            return true;
        }

        static bool IsServiceDocumentOrMetadataRoute( RouteValueDictionary values ) =>
            values.TryGetValue( "odataPath", out var value ) && ( value == null || Equals( value, "$metadata" ) );

        static bool TryGetRequestedApiVersion( HttpContext httpContext, IApiVersioningFeature feature, out ApiVersion apiVersion )
        {
            Contract.Requires( httpContext != null );
            Contract.Requires( feature != null );

            try
            {
                apiVersion = feature.RequestedApiVersion;
            }
            catch ( AmbiguousApiVersionException )
            {
                apiVersion = default;
                return false;
            }

            return true;
        }
    }
}