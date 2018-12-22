namespace Microsoft.AspNetCore.Mvc.Conventions.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;

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

    public class Order
    {
        public int Id { get; set; }

        public string Customer { get; set; }
    }
}