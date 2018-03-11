namespace Microsoft.Web.Http.Versioning
{
    using System;
    using System.Net.Http;
    using System.Web.Http;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    public partial class ApiVersionRequestProperties
    {
        readonly HttpRequestMessage request;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionRequestProperties"/> class.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage">HTTP request</see>.</param>
        public ApiVersionRequestProperties( HttpRequestMessage request )
        {
            Arg.NotNull( request, nameof( request ) );

            this.request = request;
        }

        string GetRawApiVersion()
        {
            var options = request.GetApiVersioningOptions();
            var reader = options.ApiVersionReader;
            return reader.Read( request );
        }
    }
}