namespace Microsoft.Web.Http.Simulators.V1
{
    using Microsoft.AspNet.OData;
    using Microsoft.Web.Http.Description;
    using Microsoft.Web.Http.Simulators.Models;
    using System.Web.Http;
    using System.Web.Http.Description;

    public class OrdersController : ODataController
    {
        [ResponseType( typeof( ODataValue<Order> ) )]
        public IHttpActionResult Get( int key ) => Ok( new Order() { Id = key } );

        [ResponseType( typeof( ODataValue<Order> ) )]
        public IHttpActionResult Post( [FromBody] Order order )
        {
            order.Id = 42;
            return Created( order );
        }
    }
}