namespace Microsoft.AspNet.OData.Conventions.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Models;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNet.OData.Routing;
    using System.Web.Http;

    [ODataRoutePrefix( "Orders" )]
    public class OrdersController : ODataController
    {
        [ODataRoute]
        public IHttpActionResult Get( ODataQueryOptions<Order> options ) =>
            Ok( new[] { new Order() { Id = 1, Customer = "Bill Mei" } } );

        [ODataRoute( "({key})" )]
        public IHttpActionResult Get( [FromODataUri] int key, ODataQueryOptions<Order> options ) =>
            Ok( new Order() { Id = key, Customer = "Bill Mei" } );
    }
}