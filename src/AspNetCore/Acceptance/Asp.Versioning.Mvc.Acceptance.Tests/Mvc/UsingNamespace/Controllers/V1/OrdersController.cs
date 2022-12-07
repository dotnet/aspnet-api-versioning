// Copyright (c) .NET Foundation and contributors. All rights reserved.


namespace Asp.Versioning.Mvc.UsingNamespace.Controllers.V1;

using Asp.Versioning.Mvc.UsingNamespace.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route( "api/[controller]" )]
public class OrdersController : ControllerBase
{
    [HttpGet( "{id}" )]
    public virtual IActionResult Get( int id ) => Ok();

    [HttpPost]
    public virtual IActionResult Post( [FromBody] Order order )
    {
        order.Id = 42;
        return CreatedAtAction( nameof( Get ), new { id = order.Id }, order );
    }

    [HttpPut( "{id}" )]
    public virtual IActionResult Put( int id, [FromBody] Order order ) => NoContent();

    [HttpDelete( "{id}" )]
    [ApiVersionNeutral]
    public virtual IActionResult Delete( int id ) => NoContent();
}