// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using Microsoft.AspNet.OData;
using Microsoft.OData;
using System.Net;
using System.Net.Http;
using System.Web.Http;

/// <summary>
/// Represents a <see cref="MetadataController">controller</see> for generating versioned OData service and metadata documents.
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
    ///  <item>
    ///   <term>Sunset</term>
    ///   <description>The sunset date and time of the API in RFC 1123 format, if any.</description>
    ///  </item>
    ///  <item>
    ///   <term>Link</term>
    ///   <description>Zero or more related RFC 8288 web links.</description>
    ///  </item>
    /// </list>
    /// </remarks>
    [HttpOptions]
    public virtual IHttpActionResult GetOptions()
    {
        var response = new HttpResponseMessage( HttpStatusCode.OK )
        {
            Content = new StringContent( string.Empty )
            {
                Headers = { ContentType = null },
            },
        };

        response.Content.Headers.Add( "Allow", new[] { "GET", "OPTIONS" } );
        response.Headers.Add(
            ODataConstants.ODataVersionHeader,
            ODataUtils.ODataVersionToString( ODataVersion.V4 ) );

        return ResponseMessage( response );
    }
}