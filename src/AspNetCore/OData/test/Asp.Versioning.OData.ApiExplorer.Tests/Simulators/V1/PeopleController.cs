// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.V1;

using Asp.Versioning.Simulators.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using static Microsoft.AspNetCore.Http.StatusCodes;

/// <summary>
/// Represents a RESTful people service.
/// </summary>
[ApiVersion( 1.0 )]
[ApiVersion( 0.9, Deprecated = true )]
public class PeopleController : ODataController
{
    /// <summary>
    /// Gets a single person.
    /// </summary>
    /// <param name="key">The requested person identifier.</param>
    /// <returns>The requested person.</returns>
    /// <response code="200">The person was successfully retrieved.</response>
    /// <response code="404">The person does not exist.</response>
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Person ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Get( int key ) =>
        Ok( new Person()
        {
            Id = key,
            FirstName = "John",
            LastName = "Doe",
        } );
}