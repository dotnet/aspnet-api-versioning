#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using Routing;
    using System;
    using static ApiVersion;

    /// <summary>
    /// Represents current API versioning request properties.
    /// </summary>
    public partial class ApiVersionRequestProperties
    {
        string rawApiVersion;
        ApiVersion apiVersion;

        /// <summary>
        /// Gets or sets the raw, unparsed API version for the current request.
        /// </summary>
        /// <value>The unparsed API version value for the current request.</value>
        public string RawApiVersion
        {
            get => rawApiVersion ?? ( rawApiVersion = GetRawApiVersion() );
            set => rawApiVersion = value;
        }

        /// <summary>
        /// Gets or sets the API version for the current request.
        /// </summary>
        /// <value>The current <see cref="ApiVersion">API version</see> for the current request.</value>
        /// <remarks>If an API version was not provided for the current request or the value
        /// provided is invalid, this property will return <c>null</c>.</remarks>
        public ApiVersion ApiVersion
        {
            get
            {
                if ( apiVersion == null )
                {
#pragma warning disable CA1806 // Do not ignore method results
                    TryParse( RawApiVersion, out apiVersion );
#pragma warning restore CA1806
                }

                return apiVersion;
            }
            set => apiVersion = value;
        }
    }
}