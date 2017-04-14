namespace Microsoft.Examples.V3.Controllers
{
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.OData;
    using System.Web.OData.Routing;
    using static System.Net.HttpStatusCode;

    /// <summary>
    /// Represents a RESTful service of orders.
    /// </summary>
    [ApiVersion( "3.0" )]
    [ODataRoutePrefix( "Orders" )]
    public class OrdersController : ODataController
    {
        /// <summary>
        /// Retrieves all orders.
        /// </summary>
        /// <returns>All available orders.</returns>
        /// <response code="200">Orders successfully retrieved.</response>
        /// <response code="400">The order is invalid.</response>
        [HttpGet]
        [ODataRoute]
        [ResponseType( typeof( ODataValue<IEnumerable<Order>> ) )]
        public IHttpActionResult Get()
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
        [HttpGet]
        [ODataRoute( "({id})" )]
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
        [ODataRoute]
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
        /// Cancels an order.
        /// </summary>
        /// <param name="id">The order to cancel.</param>
        /// <param name="suspendOnly">Indicates if the order should only be suspended.</param>
        /// <returns>None</returns>
        [HttpDelete]
        [ODataRoute( "({id})" )]
        public IHttpActionResult Delete( int id, bool suspendOnly ) => StatusCode( NoContent );
    }
}