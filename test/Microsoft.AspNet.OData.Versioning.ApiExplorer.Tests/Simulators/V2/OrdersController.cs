namespace Microsoft.Web.Http.Simulators.V2
{
    using Microsoft.AspNet.OData;
    using Microsoft.Web.Http.Description;
    using Microsoft.Web.Http.Simulators.Models;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.Description;
    using static System.Linq.Enumerable;

    public class OrdersController : ODataController
    {
        [ResponseType( typeof( ODataValue<IEnumerable<Order>> ) )]
        public IHttpActionResult Get() => Ok( Empty<Order>() );

        [ResponseType( typeof( ODataValue<Order> ) )]
        public IHttpActionResult Get( int id ) => Ok( new Order() { Id = id } );

        [ResponseType( typeof( ODataValue<Order> ) )]
        public IHttpActionResult Post( [FromBody] Order order )
        {
            order.Id = 42;
            return Created( order );
        }
    }
}