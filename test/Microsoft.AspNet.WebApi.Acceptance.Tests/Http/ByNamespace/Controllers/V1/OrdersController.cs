namespace Microsoft.Web.Http.ByNamespace.Controllers.V1
{
    using Microsoft.Web.Http.ByNamespace.Models;
    using System.Web.Http;
    using static System.Net.HttpStatusCode;

    [RoutePrefix( "api/orders" )]
    public class OrdersController : ApiController
    {
        [Route( "{id}", Name = "GetOrderByIdV1" )]
        public virtual IHttpActionResult Get( int id ) => Ok();

        [Route]
        public virtual IHttpActionResult Post( [FromBody] Order order )
        {
            order.Id = 42;
            return CreatedAtRoute( "GetOrderByIdV1", new { id = order.Id }, order );
        }

        [Route( "{id}" )]
        public virtual IHttpActionResult Put( int id, [FromBody] Order order ) => StatusCode( NoContent );

        [Route( "{id}" )]
        [ApiVersionNeutral]
        public IHttpActionResult Delete( int id ) => StatusCode( NoContent );
    }
}