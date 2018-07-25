namespace Microsoft.AspNet.OData.Advanced.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Models;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.Web.Http;
    using System.Web.Http;

    [ApiVersion( "2.0" )]
    [ControllerName( "Orders" )]
    [ODataRoutePrefix( "Orders" )]
    public class Orders2Controller : ODataController
    {
        [ODataRoute]
        public IHttpActionResult Get( ODataQueryOptions<Order> options ) =>
            Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } } );

        [ODataRoute( "({key})" )]
        public IHttpActionResult Get( [FromODataUri] int key, ODataQueryOptions<Order> options ) =>
            Ok( new Order() { Id = key, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } );
    }
}