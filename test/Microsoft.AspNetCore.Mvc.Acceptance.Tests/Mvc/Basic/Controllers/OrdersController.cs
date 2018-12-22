namespace Microsoft.AspNetCore.Mvc.Basic.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;

    [ApiController]
    [Route( "api/[controller]" )]
    public class OrdersController : ControllerBase
    {
        [HttpGet]
        [ApiVersion( "1.0" )]
        [ApiVersion( "2.0" )]
        public IActionResult Get() => Ok();

        [HttpGet( "{id}" )]
        [ApiVersion( "0.9" )]
        [ApiVersion( "1.0" )]
        [ApiVersion( "2.0" )]
        public IActionResult Get( int id ) => Ok();

        [HttpPost]
        [ApiVersion( "1.0" )]
        [ApiVersion( "2.0" )]
        public IActionResult Post( [FromBody] Order order )
        {
            order.Id = 42;
            return CreatedAtAction( nameof( Get ), new { id = order.Id }, order );
        }

        [HttpPut( "{id}" )]
        [ApiVersion( "2.0" )]
        public IActionResult Put( int id, [FromBody] Order order ) => NoContent();

        [HttpDelete( "{id}" )]
        [ApiVersionNeutral]
        public IActionResult Delete( int id ) => NoContent();
    }

    public class Order
    {
        public int Id { get; set; }

        public string Customer { get; set; }
    }
}