﻿namespace Microsoft.Examples.V1
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.Examples.Models;
    using Microsoft.Web.Http;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Description;
    using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;

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
        [HttpGet]
        [ODataRoute( "({key})" )]
        [ResponseType( typeof( Order ) )]
        [EnableQuery( AllowedQueryOptions = Select )]
        public SingleResult<Order> Get( int key ) => SingleResult.Create( new[] { new Order() { Id = key, Customer = "John Doe" } }.AsQueryable() );

        /// <summary>
        /// Places a new order.
        /// </summary>
        /// <param name="order">The order to place.</param>
        /// <returns>The created order.</returns>
        /// <response code="201">The order was successfully placed.</response>
        /// <response code="400">The order is invalid.</response>
        [HttpPost]
        [ODataRoute]
        [MapToApiVersion( "1.0" )]
        [ResponseType( typeof( Order ) )]
        public IHttpActionResult Post( [FromBody] Order order )
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
        [ODataRoute( nameof( MostExpensive ) )]
        [MapToApiVersion( "1.0" )]
        [ResponseType( typeof( Order ) )]
        [EnableQuery( AllowedQueryOptions = Select )]
        public SingleResult<Order> MostExpensive() => SingleResult.Create( new[] { new Order() { Id = 42, Customer = "Bill Mei" } }.AsQueryable() );
    }
}