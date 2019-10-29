namespace Microsoft.Web.Http.Description
{
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Web.Http;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    public partial class ApiExplorerOptions
    {
        readonly Lazy<ApiVersioningOptions> versioningOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiExplorerOptions"/> class.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see> associated with the options.</param>
        public ApiExplorerOptions( HttpConfiguration configuration ) =>
            versioningOptions = new Lazy<ApiVersioningOptions>( configuration.GetApiVersioningOptions );

        /// <summary>
        /// Gets the default API version applied to services that do not have explicit versions.
        /// </summary>
        /// <value>The default <see cref="ApiVersion">API version</see>.</value>
        public ApiVersion DefaultApiVersion => versioningOptions.Value.DefaultApiVersion;

        /// <summary>
        /// Gets a value indicating whether a default version is assumed when a client does
        /// does not provide a service API version.
        /// </summary>
        /// <value>True if the a default API version should be assumed when a client does not
        /// provide a service API version; otherwise, false. The default value derives from
        /// <see cref="ApiVersioningOptions.AssumeDefaultVersionWhenUnspecified"/>.</value>
        public bool AssumeDefaultVersionWhenUnspecified => versioningOptions.Value.AssumeDefaultVersionWhenUnspecified;

        /// <summary>
        /// Gets the source for defining API version parameters.
        /// </summary>
        /// <value>The <see cref="IApiVersionParameterSource">API version parameter source</see> used to describe API version parameters.</value>
        public IApiVersionParameterSource ApiVersionParameterSource => versioningOptions.Value.ApiVersionReader;
    }
}