namespace Microsoft.Web.Http.Conventions.Controllers
{
    using System.Web.Http;
    using static System.Net.HttpStatusCode;

    [RoutePrefix( "api/orders" )]
    public class OrdersController : ApiController
    {
        [Route]
        public IHttpActionResult Get() => Ok();

        [Route( "{id}", Name = "GetOrderById" )]
        public IHttpActionResult Get( int id ) => Ok();

        [Route]
        public IHttpActionResult Post( [FromBody] Order order )
        {
            order.Id = 42;
            return CreatedAtRoute( "GetOrderById", new { id = order.Id }, order );
        }

        [Route( "{id}" )]
        public IHttpActionResult Put( int id, [FromBody] Order order ) => StatusCode( NoContent );

        [Route( "{id}" )]
        public IHttpActionResult Delete( int id ) => StatusCode( NoContent );
    }

    public class Order
    {
        public int Id { get; set; }

        public string Customer { get; set; }
    }
}