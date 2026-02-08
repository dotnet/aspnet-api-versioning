namespace ApiVersioning.Examples.V3.Controllers;

using ApiVersioning.Examples.V3.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Represents a RESTful service of orders.
/// </summary>
[ApiVersion( 3.0 )]
[Route( "api/[controller]" )]
public class OrdersController : ControllerBase
{
    /// <summary>
    /// Get Orders
    /// </summary>
    /// <remarks>Retrieves all orders.</remarks>
    /// <returns>All available orders.</returns>
    /// <response code="200">Orders successfully retrieved.</response>
    [HttpGet]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( IEnumerable<Order> ), 200 )]
    public IActionResult Get()
    {
        var orders = new Order[]
        {
            new(){ Id = 1, Customer = "John Doe" },
            new(){ Id = 2, Customer = "Bob Smith" },
            new(){ Id = 3, Customer = "Jane Doe", EffectiveDate = DateTimeOffset.UtcNow.AddDays( 7d ) },
        };

        return Ok( orders );
    }

    /// <summary>
    /// Get Order
    /// </summary>
    /// <remarks>Gets a single order.</remarks>
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
    /// Place Order
    /// </summary>
    /// <remarks>Places a new order.</remarks>
    /// <param name="order">The order to place.</param>
    /// <returns>The created order.</returns>
    /// <response code="201">The order was successfully placed.</response>
    /// <response code="400">The order is invalid.</response>
    [HttpPost]
    [Consumes( "application/json" )]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Order ), 201 )]
    [ProducesResponseType( 400 )]
    public IActionResult Post( [FromBody] Order order )
    {
        order.Id = 42;
        return CreatedAtAction( nameof( Get ), new { id = order.Id }, order );
    }

    /// <summary>
    /// Cancel Order
    /// </summary>
    /// <remarks>Cancels an order.</remarks>
    /// <param name="id">The order to cancel.</param>
    /// <returns>None</returns>
    [HttpDelete( "{id:int}" )]
    [ProducesResponseType( 204 )]
    public IActionResult Delete( int id ) => NoContent();
}