namespace Microsoft.AspNetCore.Mvc.ByNamespace.Controllers.V3
{
    using Microsoft.AspNetCore.Mvc.ByNamespace.Models;
    using System;

    [Route( "api/[controller]" )]
    public class OrdersController : V2.OrdersController
    {
        [HttpGet]
        public override IActionResult Get() => Ok();

        [HttpGet( "{id}", Name = "GetOrderByIdV3" )]
        public override IActionResult Get( int id ) => Ok();

        [HttpPost]
        public override IActionResult Post( [FromBody] Order order )
        {
            order.Id = 3;
            return CreatedAtRoute( "GetOrderByIdV3", new { id = order.Id }, order );
        }

        [HttpPut( "{id}" )]
        public override IActionResult Put( int id, [FromBody] Order order ) => NoContent();
    }
}