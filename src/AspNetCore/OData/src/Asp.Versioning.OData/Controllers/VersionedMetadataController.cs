// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using static Microsoft.OData.ODataConstants;
using static Microsoft.OData.ODataUtils;
using static Microsoft.OData.ODataVersion;

/// <summary>
/// Represents a <see cref="MetadataController">controller</see> for generating versioned OData service and metadata documents.
/// </summary>
[CLSCompliant( false )]
[ReportApiVersions]
[ControllerName( "OData" )]
public class VersionedMetadataController : MetadataController
{
    private static readonly string[] values = ["GET", "OPTIONS"];

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
    public virtual IActionResult GetOptions()
    {
        var headers = Response.Headers;

        headers.Allow = new( values );
        headers[ODataVersionHeader] = ODataVersionToString( V4 );

        return Ok();
    }
}