namespace Microsoft.Examples.Controllers
{
    using Models;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;

    [ODataRoutePrefix( "Orders" )]
    public class OrdersController : ODataController
    {
        // GET ~/v1/orders
        // GET ~/orders?api-version=1.0
        [ODataRoute]
        public IHttpActionResult Get( ODataQueryOptions<Order> options ) =>
            Ok( new[] { new Order() { Id = 1, Customer = "Bill Mei" } } );

        // GET ~/v1/orders(1)
        // GET ~/orders(1)?api-version=1.0
        [ODataRoute( "({id})" )]
        public IHttpActionResult Get( [FromODataUri] int id, ODataQueryOptions<Order> options ) =>
            Ok( new Order() { Id = id, Customer = "Bill Mei" } );
    }
}