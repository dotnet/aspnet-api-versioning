namespace ApiVersioning.Examples.V3;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

/// <summary>
/// Represents a RESTful service of orders.
/// </summary>
[ApiVersion( 3.0 )]
public class OrdersController : ODataController
{
    /// <summary>
    /// Retrieves all orders.
    /// </summary>
    /// <returns>All available orders.</returns>
    /// <response code="200">Orders successfully retrieved.</response>
    /// <response code="400">The order is invalid.</response>
    [HttpGet]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ODataValue<IEnumerable<Order>> ), Status200OK )]
    [EnableQuery( MaxTop = 100, AllowedQueryOptions = Select | Top | Skip | Count )]
    public IQueryable<Order> Get()
    {
        var orders = new Order[]
        {
            new(){ Id = 1, Customer = "John Doe" },
            new(){ Id = 2, Customer = "John Doe" },
            new(){ Id = 3, Customer = "Jane Doe", EffectiveDate = DateTime.UtcNow.AddDays( 7d ) },
        };

        return orders.AsQueryable();
    }

    /// <summary>
    /// Gets a single order.
    /// </summary>
    /// <param name="key">The requested order identifier.</param>
    /// <returns>The requested order.</returns>
    /// <response code="200">The order was successfully retrieved.</response>
    /// <response code="404">The order does not exist.</response>
    [HttpGet]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Order ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    [EnableQuery( AllowedQueryOptions = Select )]
    public SingleResult<Order> Get( int key ) =>
        SingleResult.Create( new[] { new Order() { Id = key, Customer = "John Doe" } }.AsQueryable() );

    /// <summary>
    /// Places a new order.
    /// </summary>
    /// <param name="order">The order to place.</param>
    /// <returns>The created order.</returns>
    /// <response code="201">The order was successfully placed.</response>
    /// <response code="400">The order is invalid.</response>
    [HttpPost]
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
    /// Updates an existing order.
    /// </summary>
    /// <param name="key">The requested order identifier.</param>
    /// <param name="delta">The partial order to update.</param>
    /// <returns>The created order.</returns>
    /// <response code="204">The order was successfully updated.</response>
    /// <response code="400">The order is invalid.</response>
    /// <response code="404">The order does not exist.</response>
    [HttpPatch]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Order ), Status200OK )]
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status400BadRequest )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Patch( int key, [FromBody] Delta<Order> delta )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        var order = new Order() { Id = 42, Customer = "Bill Mei" };

        delta.Patch( order );

        return Updated( order );
    }

    /// <summary>
    /// Cancels an order.
    /// </summary>
    /// <param name="key">The order to cancel.</param>
    /// <param name="suspendOnly">Indicates if the order should only be suspended.</param>
    /// <returns>None</returns>
    /// <response code="204">The order was successfully canceled.</response>
    /// <response code="404">The order does not exist.</response>
    [HttpDelete]
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Delete( int key, bool suspendOnly ) => NoContent();

    /// <summary>
    /// Gets the most expensive order.
    /// </summary>
    /// <returns>The most expensive order.</returns>
    /// <response code="200">The order was successfully retrieved.</response>
    /// <response code="404">The no orders exist.</response>
    [HttpGet]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Order ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    [EnableQuery( AllowedQueryOptions = Select )]
    public SingleResult<Order> MostExpensive() =>
        SingleResult.Create( new[] { new Order() { Id = 42, Customer = "Bill Mei" } }.AsQueryable() );

    /// <summary>
    /// Rates an order.
    /// </summary>
    /// <param name="key">The requested order identifier.</param>
    /// <param name="parameters">The action parameters.</param>
    /// <returns>None</returns>
    /// <response code="204">The order was successfully rated.</response>
    /// <response code="400">The parameters are invalid.</response>
    /// <response code="404">The order does not exist.</response>
    [HttpPost]
    [ProducesResponseType( Status204NoContent )]
    [ProducesResponseType( Status400BadRequest )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Rate( int key, [FromBody] ODataActionParameters parameters )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        var rating = (int) parameters["rating"];
        return NoContent();
    }

    /// <summary>
    /// Gets the line items for the specified order.
    /// </summary>
    /// <param name="key">The order identifier.</param>
    /// <returns>The order line items.</returns>
    /// <response code="200">The line items were successfully retrieved.</response>
    /// <response code="404">The order does not exist.</response>
    [HttpGet]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ODataValue<IEnumerable<LineItem>> ), Status200OK )]
    [ProducesResponseType( Status404NotFound )]
    [EnableQuery( AllowedQueryOptions = Select | Count )]
    public IActionResult GetLineItems( int key )
    {
        var lineItems = new LineItem[]
        {
            new() { Number = 1, Quantity = 1, UnitPrice = 2m, Description = "Dry erase wipes" },
            new() { Number = 2, Quantity = 1, UnitPrice = 3.5m, Description = "Dry erase eraser" },
            new() { Number = 3, Quantity = 1, UnitPrice = 5m, Description = "Dry erase markers" },
        };

        return Ok( lineItems );
    }
}