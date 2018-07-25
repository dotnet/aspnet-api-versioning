namespace Microsoft.AspNet.OData
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using System;
    using static Microsoft.OData.ODataConstants;
    using static Microsoft.OData.ODataUtils;
    using static Microsoft.OData.ODataVersion;

    /// <summary>
    /// Represents a <see cref="MetadataController">controller</see> for generating versioned OData service and metadata documents.
    /// </summary>
    [CLSCompliant( false )]
    [ReportApiVersions]
    public class VersionedMetadataController : MetadataController
    {
        /// <summary>
        /// Handles a request for the HTTP OPTIONS method.
        /// </summary>
        /// <returns>A <see cref="IActionResult">result</see> containing the response to the request.</returns>
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
        public virtual IActionResult GetOptions()
        {
            var headers = Response.Headers;

            headers.Add( "Allow", new StringValues( new[] { "GET", "OPTIONS" } ) );
            headers.Add( ODataVersionHeader, ODataVersionToString( V4 ) );

            return Ok();
        }
    }
}