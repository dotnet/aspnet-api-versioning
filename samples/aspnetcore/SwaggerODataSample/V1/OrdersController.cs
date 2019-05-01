namespace Microsoft.Examples.V1
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Examples.Models;
    using System.Collections.Generic;
    using System.Linq;
    using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
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
        [ODataRoute( "({key})" )]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Order ), Status200OK )]
        [ProducesResponseType( Status404NotFound )]
        [EnableQuery( AllowedQueryOptions = Select )]
        public SingleResult<Order> Get( int key ) => SingleResult.Create( new[] { new Order() { Id = key, Customer = "John Doe" } }.AsQueryable() );

        /// <summary>
        /// Places a new order.
        /// </summary>
        /// <param name="order">The order to place.</param>
        /// <returns>The created order.</returns>
        /// <response code="201">The order was successfully placed.</response>
        /// <response code="400">The order is invalid.</response>
        [ODataRoute]
        [MapToApiVersion( "1.0" )]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Order ), Status201Created )]
        [ProducesResponseType( Status400BadRequest )]
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
        [ODataRoute( "MostExpensive" )]
        [MapToApiVersion( "1.0" )]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( Order ), Status200OK )]
        [ProducesResponseType( Status404NotFound )]
        [EnableQuery( AllowedQueryOptions = Select )]
        public SingleResult<Order> MostExpensive() => SingleResult.Create( new[] { new Order() { Id = 42, Customer = "Bill Mei" } }.AsQueryable() );

        /// <summary>
        /// Gets the line items for the specified order.
        /// </summary>
        /// <param name="key">The order identifier.</param>
        /// <returns>The order line items.</returns>
        /// <response code="200">The line items were successfully retrieved.</response>
        /// <response code="404">The order does not exist.</response>
        [HttpGet]
        [ODataRoute( "({key})/LineItems" )]
        [Produces( "application/json" )]
        [ProducesResponseType( typeof( ODataValue<IEnumerable<LineItem>> ), Status200OK )]
        [ProducesResponseType( Status404NotFound )]
        [EnableQuery( AllowedQueryOptions = Select )]
        public IActionResult LineItems( int key )
        {
            var lineItems = new[]
            {
                new LineItem() { Number = 1, Quantity = 1, UnitPrice = 2m, Description = "Dry erase wipes" },
                new LineItem() { Number = 2, Quantity = 1, UnitPrice = 3.5m, Description = "Dry erase eraser" },
                new LineItem() { Number = 3, Quantity = 1, UnitPrice = 5m, Description = "Dry erase markers" },
            };

            return Ok( lineItems );
        }
    }
}