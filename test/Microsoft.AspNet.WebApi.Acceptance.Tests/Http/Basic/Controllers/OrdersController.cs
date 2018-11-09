namespace Microsoft.Web.Http.Basic.Controllers
{
    using Microsoft.Web.Http;
    using System.Web.Http;
    using static System.Net.HttpStatusCode;

    [RoutePrefix( "api/orders" )]
    public class OrdersController : ApiController
    {
        [Route]
        [ApiVersion( "1.0" )]
        [ApiVersion( "2.0" )]
        public IHttpActionResult Get() => Ok();

        [Route( "{id}", Name = "GetOrderById" )]
        [ApiVersion( "0.9" )]
        [ApiVersion( "1.0" )]
        [ApiVersion( "2.0" )]
        public IHttpActionResult Get( int id ) => Ok();

        [Route]
        [ApiVersion( "1.0" )]
        [ApiVersion( "2.0" )]
        public IHttpActionResult Post( [FromBody] Order order )
        {
            order.Id = 42;
            return CreatedAtRoute( "GetOrderById", new { id = order.Id }, order );
        }

        [Route( "{id}" )]
        [ApiVersion( "2.0" )]
        public IHttpActionResult Put( int id, [FromBody] Order order ) => StatusCode( NoContent );

        [Route( "{id}" )]
        [ApiVersionNeutral]
        public IHttpActionResult Delete( int id ) => StatusCode( NoContent );
    }

    public class Order
    {
        public int Id { get; set; }

        public string Customer { get; set; }
    }
}