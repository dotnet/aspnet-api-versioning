namespace Microsoft.Web.OData.Routing
{
    using Http;
    using Http.Versioning;
    using Microsoft.OData.Core;
    using Microsoft.OData.Edm;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using System.Web.OData.Routing;
    using System.Web.OData.Routing.Conventions;
    using static System.Net.HttpStatusCode;
    using static System.Web.Http.Routing.HttpRouteDirection;

    /// <summary>
    /// Represents an <see cref="ODataPathRouteConstraint">OData path route constraint</see> which supports versioning.
    /// </summary>
    public class VersionedODataPathRouteConstraint : ODataPathRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedODataPathRouteConstraint" /> class.
        /// </summary>
        /// <param name="pathHandler">The OData path handler to use for parsing.</param>
        /// <param name="model">The EDM model to use for parsing the path.</param>
        /// <param name="routeName">The name of the route this constraint is associated with.</param>
        /// <param name="routingConventions">The OData routing conventions to use for selecting the controller name.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the route constraint.</param>
        public VersionedODataPathRouteConstraint(
            IODataPathHandler pathHandler,
            IEdmModel model,
            string routeName,
            IEnumerable<IODataRoutingConvention> routingConventions,
            ApiVersion apiVersion )
            : base( pathHandler, model, routeName, routingConventions )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            ApiVersion = apiVersion;
        }

        private static bool IsServiceDocumentOrMetadataRoute( IDictionary<string, object> values )
        {
            Contract.Requires( values != null );
            object value;
            return values.TryGetValue( "odataPath", out value ) && ( value == null || Equals( value, "$metadata" ) );
        }

        /// <summary>
        /// Gets the API version matched by the current OData path route constraint.
        /// </summary>
        /// <value>The <see cref="ApiVersion">API version</see> associated with the route constraint.</value>
        public ApiVersion ApiVersion { get; }

        /// <summary>
        /// Determines whether this instance equals a specified route.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="route">The route to compare.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="values">A list of parameter values.</param>
        /// <param name="routeDirection">The route direction.</param>
        /// <returns>True if this instance equals a specified route; otherwise, false.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public override bool Match( HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection )
        {
            Arg.NotNull( request, nameof( request ) );
            Arg.NotNull( values, nameof( values ) );

            if ( routeDirection == UriGeneration )
            {
                return base.Match( request, route, parameterName, values, routeDirection );
            }

            var properties = request.ApiVersionProperties();
            var requestedVersion = GetRequestedApiVersionOrReturnBadRequest( request, properties );

            if ( requestedVersion != null )
            {
                if ( ApiVersion == requestedVersion && base.Match( request, route, parameterName, values, routeDirection ) )
                {
                    DecorateUrlHelperWithApiVersionRouteValueIfNecessary( request, values );
                    return true;
                }

                return false;
            }

            var options = request.GetApiVersioningOptions();

            if ( options.DefaultApiVersion != ApiVersion )
            {
                return false;
            }

            if ( options.AssumeDefaultVersionWhenUnspecified || IsServiceDocumentOrMetadataRoute( values ) )
            {
                properties.ApiVersion = ApiVersion;
                return base.Match( request, route, parameterName, values, routeDirection );
            }

            return false;
        }

        static ApiVersion GetRequestedApiVersionOrReturnBadRequest( HttpRequestMessage request, ApiVersionRequestProperties properties )
        {
            Contract.Requires( request != null );
            Contract.Requires( properties != null );

            try
            {
                return properties.ApiVersion;
            }
            catch ( AmbiguousApiVersionException ex )
            {
                var error = new ODataError() { ErrorCode = "AmbiguousApiVersion", Message = ex.Message };
                throw new HttpResponseException( request.CreateResponse( BadRequest, error ) );
            }
        }

        static void DecorateUrlHelperWithApiVersionRouteValueIfNecessary( HttpRequestMessage request, IDictionary<string, object> values )
        {
            Contract.Requires( request != null );
            Contract.Requires( values != null );

            var apiVersion = default( object );

            if ( !values.TryGetValue( nameof( apiVersion ), out apiVersion ) )
            {
                return;
            }

            var requestContext = request.GetRequestContext();

            if ( !( requestContext.Url is VersionedUrlHelperDecorator ) )
            {
                requestContext.Url = new VersionedUrlHelperDecorator( requestContext.Url, apiVersion );
            }
        }
    }
}