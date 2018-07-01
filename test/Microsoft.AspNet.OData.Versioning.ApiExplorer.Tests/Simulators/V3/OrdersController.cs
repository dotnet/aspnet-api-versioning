namespace Microsoft.Web.Http.Simulators.V3
{
    using Microsoft.AspNet.OData;
    using Microsoft.Web.Http.Description;
    using Microsoft.Web.Http.Simulators.Models;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.Description;
    using static System.Linq.Enumerable;
    using static System.Net.HttpStatusCode;

    public class OrdersController : ODataController
    {
        [ResponseType( typeof( ODataValue<IEnumerable<Order>> ) )]
        public IHttpActionResult Get() => Ok( Empty<Person>() );

        [ResponseType( typeof( ODataValue<Order> ) )]
        public IHttpActionResult Get( int key ) => Ok( new Order() { Id = key } );

        [ResponseType( typeof( ODataValue<Order> ) )]
        public IHttpActionResult Post( [FromBody] Order order )
        {
            order.Id = 42;
            return Created( order );
        }

        public IHttpActionResult Delete( int key ) => StatusCode( NoContent );
    }
}