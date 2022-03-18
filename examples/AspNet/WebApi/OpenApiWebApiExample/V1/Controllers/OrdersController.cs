namespace ApiVersioning.Examples.V1.Controllers;

using ApiVersioning.Examples.V1.Models;
using Asp.Versioning;
using System.Web.Http;
using System.Web.Http.Description;

/// <summary>
/// Represents a RESTful service of orders.
/// </summary>
[ApiVersion( 1.0 )]
[ApiVersion( 0.9, Deprecated = true )]
[RoutePrefix( "api/orders" )]
public class OrdersController : ApiController
{
    /// <summary>
    /// Gets a single order.
    /// </summary>
    /// <param name="id">The requested order identifier.</param>
    /// <returns>The requested order.</returns>
    /// <response code="200">The order was successfully retrieved.</response>
    /// <response code="404">The order does not exist.</response>
    [HttpGet]
    [Route( "{id:int}", Name = "GetOrderById" )]
    [ResponseType( typeof( Order ) )]
    public IHttpActionResult Get( int id ) => Ok( new Order() { Id = id, Customer = "John Doe" } );

    /// <summary>
    /// Places a new order.
    /// </summary>
    /// <param name="order">The order to place.</param>
    /// <returns>The created order.</returns>
    /// <response code="201">The order was successfully placed.</response>
    /// <response code="400">The order is invalid.</response>
    [HttpPost]
    [Route]
    [MapToApiVersion( "1.0" )]
    [ResponseType( typeof( Order ) )]
    public IHttpActionResult Post( [FromBody] Order order )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        order.Id = 42;

        return CreatedAtRoute( "GetOrderById", new { id = order.Id }, order );
    }
}