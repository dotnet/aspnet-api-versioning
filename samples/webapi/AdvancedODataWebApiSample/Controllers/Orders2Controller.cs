namespace Microsoft.Examples.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.Examples.Models;
    using Microsoft.Web.Http;
    using System.Web.Http;

    [ApiVersion( "2.0" )]
    [ControllerName( "Orders" )]
    [ODataRoutePrefix( "Orders" )]
    public class Orders2Controller : ODataController
    {
        // GET ~/api/orders?api-version=2.0
        [ODataRoute]
        public IHttpActionResult Get( ODataQueryOptions<Order> options, ApiVersion version ) =>
            Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{version}" } } );

        // GET ~/api/orders/{id}?api-version=2.0
        [ODataRoute( "{id}" )]
        public IHttpActionResult Get( int id, ODataQueryOptions<Order> options, ApiVersion version ) =>
            Ok( new Order() { Id = id, Customer = $"Customer v{version}" } );
    }
}