namespace Microsoft.AspNet.OData.Conventions.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Models;
    using Microsoft.AspNet.OData.Query;
    using System.Web.Http;

    public class OrdersController : ODataController
    {
        public IHttpActionResult Get( ODataQueryOptions<Order> options ) =>
            Ok( new[] { new Order() { Id = 1, Customer = "Bill Mei" } } );

        public IHttpActionResult Get( int key, ODataQueryOptions<Order> options ) =>
            Ok( new Order() { Id = key, Customer = "Bill Mei" } );
    }
}