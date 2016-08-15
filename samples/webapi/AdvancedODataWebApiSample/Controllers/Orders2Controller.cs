namespace Microsoft.Examples.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;

    [ApiVersion( "2.0" )]
    [ControllerName( "Orders" )]
    [ODataRoutePrefix( "Orders" )]
    public class Orders2Controller : ODataController
    {
        // GET ~/orders?api-version=2.0
        [ODataRoute]
        public IHttpActionResult Get( ODataQueryOptions<Order> options ) =>
            Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } } );

        // GET ~/orders({id})?api-version=2.0
        [ODataRoute( "({id})" )]
        public IHttpActionResult Get( [FromODataUri] int id, ODataQueryOptions<Order> options ) =>
            Ok( new Order() { Id = id, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } );
    }
}