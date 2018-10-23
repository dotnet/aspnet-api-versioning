namespace Microsoft.AspNet.OData.Simulators.V1
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Simulators.Models;
    using Microsoft.AspNetCore.Mvc;
    using static Microsoft.AspNetCore.Http.StatusCodes;

    /// <summary>
    /// Represents a RESTful service of orders.
    /// </summary>
    [ApiVersion( "1.0" )]
    [ApiVersion( "0.9", Deprecated = true )]
    [ODataRoutePrefix( "Orders" )]
    public class OrdersController : ODataController
    {
        /// <summary>
        /// Gets a single order.
        /// </summary>
        /// <param name="key">The requested order identifier.</param>
        /// <returns>The requested order.</returns>
        /// <response code="200">The order was successfully retrieved.</response>
        /// <response code="404">The order does not exist.</response>
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Order ), Status200OK )]
        [ProducesResponseType( Status404NotFound )]
        [ODataRoute( "({key})" )]
        public IActionResult Get( int key ) => Ok( new Order() { Id = key, Customer = "John Doe" } );

        /// <summary>
        /// Places a new order.
        /// </summary>
        /// <param name="order">The order to place.</param>
        /// <returns>The created order.</returns>
        /// <response code="201">The order was successfully placed.</response>
        /// <response code="400">The order is invalid.</response>
        [MapToApiVersion( "1.0" )]
        [ProducesResponseType( typeof( Order ), Status201Created )]
        [ProducesResponseType( Status400BadRequest )]
        [ODataRoute]
        public IActionResult Post( [FromBody] Order order )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( ModelState );
            }

            order.Id = 42;

            return Created( order );
        }

        /// <summary>
        /// Gets the most expensive order.
        /// </summary>
        /// <returns>The most expensive order.</returns>
        /// <response code="200">The order was successfully retrieved.</response>
        /// <response code="404">The no orders exist.</response>
        [HttpGet]
        [MapToApiVersion( "1.0" )]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Order ), Status200OK )]
        [ODataRoute( "MostExpensive" )]
        public IActionResult MostExpensive() => Ok( new Order() { Id = 42, Customer = "Bill Mei" } );
    }
}