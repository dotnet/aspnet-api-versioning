namespace Microsoft.Web.Http.Description
{
    using System;
    using System.Web.Http;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    public partial class ApiExplorerOptions
    {
        readonly HttpConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiExplorerOptions"/> class.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see> associated with the options.</param>
        public ApiExplorerOptions( HttpConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets or sets the default API version applied to services that do not have explicit versions.
        /// </summary>
        /// <value>The default <see cref="ApiVersion">API version</see>.</value>
        public ApiVersion DefaultApiVersion => configuration.GetApiVersioningOptions().DefaultApiVersion;
    }
}