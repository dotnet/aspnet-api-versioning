namespace Microsoft.Examples.V1.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;

    /// <summary>
    /// Represents a RESTful service of orders.
    /// </summary>
    [ApiController]
    [ApiVersion( "1.0" )]
    [ApiVersion( "0.9", Deprecated = true )]
    [Route( "api/[controller]" )]
    public class OrdersController : ControllerBase
    {
        /// <summary>
        /// Gets a single order.
        /// </summary>
        /// <param name="id">The requested order identifier.</param>
        /// <returns>The requested order.</returns>
        /// <response code="200">The order was successfully retrieved.</response>
        /// <response code="404">The order does not exist.</response>
        [HttpGet( "{id:int}" )]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Order ), 200 )]
        [ProducesResponseType( 404 )]
        public IActionResult Get( int id ) => Ok( new Order() { Id = id, Customer = "John Doe" } );

        /// <summary>
        /// Places a new order.
        /// </summary>
        /// <param name="order">The order to place.</param>
        /// <returns>The created order.</returns>
        /// <response code="201">The order was successfully placed.</response>
        /// <response code="400">The order is invalid.</response>
        [HttpPost]
        [MapToApiVersion( "1.0" )]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Order ), 201 )]
        [ProducesResponseType( 400 )]
        public IActionResult Post( [FromBody] Order order )
        {
            order.Id = 42;
            return CreatedAtAction( nameof( Get ), new { id = order.Id }, order );
        }

        /// <summary>
        /// Updates an existing order.
        /// </summary>
        /// <param name="order">The order to update.</param>
        /// <returns>The created order.</returns>
        /// <response code="204">The order was successfully updated.</response>
        /// <response code="400">The order is invalid.</response>
        /// <response code="404">The order does not exist.</response>
        [MapToApiVersion( "1.0" )]
        [HttpPatch("{id:int}")]
        [Produces("application/json")]
        [Consumes("application/merge-patch+json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult Patch(int id, [FromBody] Order order)
        {
            order.Id = 1; // if id=1 then it is returned from V1 OrdersController.cs
            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
        }
    }
}