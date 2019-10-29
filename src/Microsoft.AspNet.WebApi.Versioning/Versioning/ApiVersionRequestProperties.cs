namespace Microsoft.Web.Http.Versioning
{
    using System.ComponentModel;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using static System.ComponentModel.EditorBrowsableState;

    /// <summary>
    /// Represents current API versioning request properties.
    /// </summary>
    public class ApiVersionRequestProperties
    {
        readonly HttpRequestMessage request;
        string? rawApiVersion;
        ApiVersion? apiVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionRequestProperties"/> class.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage">HTTP request</see>.</param>
        public ApiVersionRequestProperties( HttpRequestMessage request ) => this.request = request;

        /// <summary>
        /// Gets or sets the raw, unparsed API version for the current request.
        /// </summary>
        /// <value>The unparsed API version value for the current request.</value>
        public string? RawRequestedApiVersion
        {
            get
            {
                if ( rawApiVersion == null )
                {
                    var options = request.GetApiVersioningOptions();
                    var reader = options.ApiVersionReader;
                    rawApiVersion = reader.Read( request );
                }

                return rawApiVersion;
            }
            set => rawApiVersion = value;
        }

        /// <summary>
        /// Gets or sets the API version for the current request.
        /// </summary>
        /// <value>The current <see cref="RequestedApiVersion">API version</see> for the current request.</value>
        /// <remarks>If an API version was not provided for the current request or the value
        /// provided is invalid, this property will return <c>null</c>.</remarks>
        public ApiVersion? RequestedApiVersion
        {
            get
            {
                if ( apiVersion == null )
                {
#pragma warning disable CA1806 // Do not ignore method results
                    ApiVersion.TryParse( RawRequestedApiVersion, out apiVersion );
#pragma warning restore CA1806
                }

                return apiVersion;
            }
            set => apiVersion = value;
        }

        /// <summary>
        /// Gets or sets the controller selected during a request.
        /// </summary>
        /// <value>The <see cref="HttpControllerDescriptor">controller</see> select during a request.
        /// The default value is <c>null</c>.</value>
        [EditorBrowsable( Never )]
        public HttpControllerDescriptor? SelectedController { get; set; }
    }
}