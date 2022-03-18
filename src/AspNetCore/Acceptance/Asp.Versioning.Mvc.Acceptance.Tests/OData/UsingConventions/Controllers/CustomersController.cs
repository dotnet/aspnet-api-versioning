// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.OData.UsingConventions.Controllers;

using Asp.Versioning.OData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;

public class CustomersController : ODataController
{
    public IActionResult Get() => Ok();

    public IActionResult Get( int key ) => Ok();

    public IActionResult Post( [FromBody] Customer customer )
    {
        customer.Id = 42;
        return Created( customer );
    }

    public IActionResult Put( int key, [FromBody] Customer customer ) => NoContent();

    public IActionResult Delete( int key ) => NoContent();
}