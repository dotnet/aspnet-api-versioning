namespace Microsoft.AspNetCore.Mvc.ByNamespace.Controllers.V1
{
    using Microsoft.AspNetCore.Mvc.ByNamespace.Models;
    using System;

    [Route( "api/[controller]" )]
    public class OrdersController : ControllerBase
    {
        [HttpGet( "{id}", Name = "GetOrderByIdV1" )]
        public virtual IActionResult Get( int id ) => Ok();

        [HttpPost]
        public virtual IActionResult Post( [FromBody] Order order )
        {
            order.Id = 1;
            return CreatedAtRoute( "GetOrderByIdV1", new { id = order.Id }, order );
        }

        [HttpPut( "{id}" )]
        public virtual IActionResult Put( int id, [FromBody] Order order ) => NoContent();

        [HttpDelete( "{id}" )]
        [ApiVersionNeutral]
        public virtual IActionResult Delete( int id ) => NoContent();
    }
}