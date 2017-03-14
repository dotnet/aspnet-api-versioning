namespace Microsoft.Web.OData.Controllers
{
    using Http;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.OData;
    using static Microsoft.OData.ODataConstants;
    using static Microsoft.OData.ODataUtils;
    using static Microsoft.OData.ODataVersion;
    using static System.Net.HttpStatusCode;
    using static System.String;

    /// <summary>
    /// Represents a <see cref="ApiController">controller</see> for generating versioned OData service and metadata documents.
    /// </summary>
    [ReportApiVersions]
    public class VersionedMetadataController : MetadataController
    {
        /// <summary>
        /// Handles a request for the HTTP OPTIONS method.
        /// </summary>
        /// <returns>A <see cref="IHttpActionResult">result</see> containing the response to the request.</returns>
        /// <remarks>When a request is made with OPTIONS /$metadata, then this method will return the following
        /// HTTP headers:
        /// <list type="table">
        ///  <listheader>
        ///  <term>Header Name</term>
        ///  <description>Description</description>
        ///  </listheader>
        ///  <item>
        ///   <term>OData-Version</term>
        ///   <description>The OData version supported by the endpoint.</description>
        ///  </item>
        ///  <item>
        ///   <term>api-supported-versions</term>
        ///   <description>A comma-separated list of all supported API versions, if any.</description>
        ///  </item>
        ///  <item>
        ///   <term>api-deprecated-versions</term>
        ///   <description>A comma-separated list of all supported API versions, if any.</description>
        ///  </item>
        /// </list>
        /// </remarks>
        [HttpOptions]
        public virtual IHttpActionResult GetOptions()
        {
            var response = new HttpResponseMessage( OK );
            var headers = response.Headers;

            response.Content = new StringContent( Empty );
            response.Content.Headers.Add( "Allow", new[] { "GET", "OPTIONS" } );
            response.Content.Headers.ContentType = null;
            headers.Add( ODataVersionHeader, ODataVersionToString( V4 ) );

            return ResponseMessage( response );
        }
    }
}