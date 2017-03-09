namespace Microsoft.OData.Edm
{
    using System;
    using Web.Http;

    /// <summary>
    /// Represents an annotation for <see cref="ApiVersion">API version</see>.
    /// </summary>
    public class ApiVersionAnnotation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionAnnotation"/> class.
        /// </summary>
        /// <param name="apiVersion">The annotated <see cref="ApiVersion">API version</see>.</param>
        public ApiVersionAnnotation( ApiVersion apiVersion )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            ApiVersion = apiVersion;
        }

        /// <summary>
        /// Gets the annotated API version.
        /// </summary>
        /// <value>The annotated <see cref="ApiVersion">API version</see>.</value>
        public ApiVersion ApiVersion { get; }
    }
}