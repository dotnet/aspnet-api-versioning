// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.OData.Basic.Controllers;

using Asp.Versioning.OData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;

public class CustomersController : ODataController
{
    [ApiVersion( 2.0 )]
    [ApiVersion( 3.0 )]
    public IActionResult Get() => Ok();

    [ApiVersion( 1.0 )]
    [ApiVersion( 2.0 )]
    [ApiVersion( 3.0 )]
    public IActionResult Get( int key ) => Ok();

    [ApiVersion( 1.0 )]
    [ApiVersion( 2.0 )]
    [ApiVersion( 3.0 )]
    public IActionResult Post( [FromBody] Customer customer )
    {
        customer.Id = 42;
        return Created( customer );
    }

    [ApiVersion( 3.0 )]
    public IActionResult Put( int key, [FromBody] Customer customer ) => NoContent();

    [ApiVersionNeutral]
    public IActionResult Delete( int key ) => NoContent();
}