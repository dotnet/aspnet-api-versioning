namespace Microsoft.OData.Edm
{
#if WEBAPI
    using Microsoft.Web.Http;
    using System;
    using System.Web.Http;
#else
    using Microsoft.AspNetCore.Mvc;
    using System;
#endif

    /// <summary>
    /// Represents an annotation for <see cref="ApiVersion">API version</see>.
    /// </summary>
    public class ApiVersionAnnotation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionAnnotation"/> class.
        /// </summary>
        /// <param name="apiVersion">The annotated <see cref="ApiVersion">API version</see>.</param>
        public ApiVersionAnnotation( ApiVersion apiVersion ) => ApiVersion = apiVersion;

        /// <summary>
        /// Gets the annotated API version.
        /// </summary>
        /// <value>The annotated <see cref="ApiVersion">API version</see>.</value>
        public ApiVersion ApiVersion { get; }
    }
}