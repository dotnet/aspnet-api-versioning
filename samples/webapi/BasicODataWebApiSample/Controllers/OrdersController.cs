namespace Microsoft.Examples.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;

    [ApiVersion( "1.0" )]
    [ODataRoutePrefix( "Orders" )]
    public class OrdersController : ODataController
    {
        // GET ~/v1/orders
        [ODataRoute]
        public async Task<IHttpActionResult> Get( ODataQueryOptions<Person> options ) =>
            Ok( new[] { new Order() { Id = 1, Customer = "Bill Mei" } } );

        // GET ~/v1/orders(1)
        [ODataRoute( "({id})" )]
        public async Task<IHttpActionResult> Get( [FromODataUri] int id, ODataQueryOptions<Person> options ) =>
            Ok( new Order() { Id = id, Customer = "Bill Mei" } );
    }
}