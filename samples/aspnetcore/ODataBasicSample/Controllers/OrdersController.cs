namespace Microsoft.Examples.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [ApiVersion( "1.0" )]
    [ODataRoutePrefix( "Orders" )]
    public class OrdersController : ODataController
    {
        // GET ~/v1/orders
        // GET ~/api/orders?api-version=1.0
        [ODataRoute]
        public IActionResult Get( ODataQueryOptions<Order> options ) =>
            Ok( new[] { new Order() { Id = 1, Customer = "Bill Mei" } } );

        // GET ~/v1/orders(1)
        // GET ~/api/orders(1)?api-version=1.0
        [ODataRoute( "({id})" )]
        public IActionResult Get( [FromODataUri] int id, ODataQueryOptions<Order> options ) =>
            Ok( new Order() { Id = id, Customer = "Bill Mei" } );
    }
}