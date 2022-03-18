// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.Simulators.V2;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using static Microsoft.AspNetCore.Http.StatusCodes;

/// <summary>
/// Represents a RESTful service of orders.
/// </summary>
[ApiVersion( 2.0 )]
public class OrdersController : ODataController
{
    /// <summary>
    /// Retrieves all orders.
    /// </summary>
    /// <returns>All available orders.</returns>
    /// <response code="200">The successfully retrieved orders.</response>
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( ODataValue<IEnumerable<Order>> ), Status200OK )]
    public IActionResult Get()
    {
        var orders = new Order[]
        {
            new() { Id = 1, Customer = "John Doe" },
            new() { Id = 2, Customer = "Bob Smith" },
            new() { Id = 3, Customer = "Jane Doe", EffectiveDate = DateTime.UtcNow.AddDays( 7d ) },
        };

        return Ok( orders );
    }

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
    public IActionResult Get( int key ) => Ok( new Order() { Id = key, Customer = "John Doe" } );

    /// <summary>
    /// Places a new order.
    /// </summary>
    /// <param name="order">The order to place.</param>
    /// <returns>The created order.</returns>
    /// <response code="201">The order was successfully placed.</response>
    /// <response code="400">The order is invalid.</response>
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
    /// <response code="404">The order does not exist.</response>
    [ProducesResponseType( typeof( Order ), Status204NoContent )]
    [ProducesResponseType( Status400BadRequest )]
    [ProducesResponseType( Status404NotFound )]
    public IActionResult Patch( int key, [FromBody] Delta<Order> delta )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        var order = new Order() { Id = key, Customer = "Bill Mei" };

        delta.Patch( order );

        return Updated( order );
    }

    /// <summary>
    /// Gets the most expensive order.
    /// </summary>
    /// <returns>The most expensive order.</returns>
    /// <response code="200">The order was successfully retrieved.</response>
    /// <response code="404">The no orders exist.</response>
    [HttpGet]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( Order ), Status200OK )]
    public IActionResult MostExpensive() => Ok( new Order() { Id = 42, Customer = "Bill Mei" } );

    /// <summary>
    /// Rates an order.
    /// </summary>
    /// <param name="key">The requested order identifier.</param>
    /// <param name="parameters">The action parameters.</param>
    /// <returns>None.</returns>
    /// <response code="204">The order was successfully rated.</response>
    [HttpPost]
    [ProducesResponseType( Status200OK )]
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
}