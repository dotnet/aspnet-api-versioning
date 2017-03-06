#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using Routing;
    using System;
    using System.ComponentModel;
    using static System.ComponentModel.EditorBrowsableState;
    using static ApiVersion;

    /// <summary>
    /// Represents current API versioning request properties.
    /// </summary>
    public partial class ApiVersionRequestProperties
    {
        readonly Lazy<string> rawApiVersion;
        bool apiVersionInitialized;
        ApiVersion apiVersion;

        /// <summary>
        /// Gets the raw, unparsed API version for the current request.
        /// </summary>
        /// <value>The unparsed API version value for the current request.</value>
        public string RawApiVersion => rawApiVersion.Value;

        /// <summary>
        /// Gets the API version for the current request.
        /// </summary>
        /// <value>The current <see cref="ApiVersion">API version</see> for the current request.</value>
        /// <remarks>If an API version was not provided for the current request or the value
        /// provided is invalid, this property will return <c>null</c>.</remarks>
        public ApiVersion ApiVersion
        {
            get
            {
                if ( !apiVersionInitialized )
                {
                    TryParse( RawApiVersion, out apiVersion );
                    apiVersionInitialized = true;
                }

                return apiVersion;
            }
            set
            {
                apiVersion = value;
                apiVersionInitialized = true;
            }
        }

        /// <summary>
        /// Gets or sets the route parameter name used in URL segment API versioning.
        /// </summary>
        /// <value>The route parameter name used in URL segment API versioning.</value>
        /// <remarks>This property is typically set by the <see cref="ApiVersionRouteConstraint"/>
        /// and is not meant to be directly used in your code.</remarks>
        [EditorBrowsable( Never )]
        public string RouteParameterName { get; set; }
    }
}