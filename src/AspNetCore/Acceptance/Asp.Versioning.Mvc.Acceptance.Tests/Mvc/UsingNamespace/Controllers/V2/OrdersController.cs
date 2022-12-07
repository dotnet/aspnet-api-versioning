// Copyright (c) .NET Foundation and contributors. All rights reserved.


namespace Asp.Versioning.Mvc.UsingNamespace.Controllers.V2;

using Asp.Versioning.Mvc.UsingNamespace.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route( "api/[controller]" )]
public class OrdersController : V1.OrdersController
{
    [HttpGet]
    public virtual IActionResult Get() => Ok();

    [HttpGet( "{id}" )]
    public override IActionResult Get( int id ) => Ok();

    [HttpPost]
    public override IActionResult Post( [FromBody] Order order )
    {
        order.Id = 42;
        return CreatedAtAction( nameof( Get ), new { id = order.Id }, order );
    }

    [HttpPut( "{id}" )]
    public override IActionResult Put( int id, [FromBody] Order order ) => NoContent();

    [NonAction]
    public override IActionResult Delete( int id ) => throw new NotImplementedException();
}