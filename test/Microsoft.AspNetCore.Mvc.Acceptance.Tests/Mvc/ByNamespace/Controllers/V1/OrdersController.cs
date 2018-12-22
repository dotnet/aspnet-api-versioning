namespace Microsoft.AspNetCore.Mvc.ByNamespace.Controllers.V1
{
    using Microsoft.AspNetCore.Mvc.ByNamespace.Models;
    using System;

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
}