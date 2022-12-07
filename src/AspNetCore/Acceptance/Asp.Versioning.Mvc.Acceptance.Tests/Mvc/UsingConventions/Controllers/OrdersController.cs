// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.Mvc.UsingConventions.Controllers;

using Asp.Versioning.Mvc.UsingConventions.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route( "api/[controller]" )]
public class OrdersController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();

    [HttpGet( "{id}" )]
    public IActionResult Get( int id ) => Ok();

    [HttpPost]
    public IActionResult Post( [FromBody] Order order )
    {
        order.Id = 42;
        return CreatedAtAction( nameof( Get ), new { id = order.Id }, order );
    }

    [HttpPut( "{id}" )]
    public IActionResult Put( int id, [FromBody] Order order ) => NoContent();

    [HttpDelete( "{id}" )]
    public IActionResult Delete( int id ) => NoContent();
}