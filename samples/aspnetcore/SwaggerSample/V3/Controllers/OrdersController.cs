namespace Microsoft.Examples.V3.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a RESTful service of orders.
    /// </summary>
    [ApiVersion( "3.0" )]
    [Route( "api/[controller]" )]
    public class OrdersController : Controller
    {
        const string ByIdRouteName = "GetOrderById-" + nameof( V3 );

        /// <summary>
        /// Retrieves all orders.
        /// </summary>
        /// <returns>All available orders.</returns>
        /// <response code="200">Orders successfully retrieved.</response>
        /// <response code="400">The order is invalid.</response>
        [HttpGet]
        [ProducesResponseType( typeof( IEnumerable<Order> ), 200 )]
        [ProducesResponseType( 400 )]
        public IActionResult Get()
        {
            var orders = new[]
            {
                new Order(){ Id = 1, Customer = "John Doe" },
                new Order(){ Id = 2, Customer = "John Doe" },
                new Order(){ Id = 3, Customer = "Jane Doe", EffectiveDate = DateTimeOffset.UtcNow.AddDays( 7d ) }
            };

            return Ok( orders );
        }

        /// <summary>
        /// Gets a single order.
        /// </summary>
        /// <param name="id">The requested order identifier.</param>
        /// <returns>The requested order.</returns>
        /// <response code="200">The order was successfully retrieved.</response>
        /// <response code="404">The order does not exist.</response>
        [HttpGet( "{id:int}", Name = ByIdRouteName )]
        [ProducesResponseType( typeof( Order ), 200 )]
        [ProducesResponseType( 400 )]
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
        [ProducesResponseType( typeof( Order ), 201 )]
        [ProducesResponseType( 400 )]
        public IActionResult Post( [FromBody] Order order )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            order.Id = 42;

            return CreatedAtRoute( ByIdRouteName, new { id = order.Id }, order );
        }

        /// <summary>
        /// Cancels an order.
        /// </summary>
        /// <param name="id">The order to cancel.</param>
        /// <returns>None</returns>
        [HttpDelete( "{id:int}" )]
        [ProducesResponseType( 204 )]
        public IActionResult Delete( int id ) => NoContent();
    }
}