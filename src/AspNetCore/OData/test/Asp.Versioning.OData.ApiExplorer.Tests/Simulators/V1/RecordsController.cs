// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.V1;

using Asp.Versioning.Simulators.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using static Microsoft.AspNetCore.Http.StatusCodes;

/// <summary>
/// Represents a RESTful record service.
/// </summary>
[ApiVersion( 1.0 )]
public class RecordsController : ODataController
{
    /// <summary>
    /// Gets a single record.
    /// </summary>
    /// <param name="id">The record identifier.</param>
    /// <param name="source">The record source identifier.</param>
    /// <returns>The requested record.</returns>
    /// <response code="200">The record was successfully retrieved.</response>
    /// <response code="404">The record does not exist.</response>
    [HttpGet( "api/Records(id={id}, source={source})" )]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Record ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Get( string id, int source ) =>
        Ok( new Record()
        {
            Id = id,
            Source = source,
        } );
}