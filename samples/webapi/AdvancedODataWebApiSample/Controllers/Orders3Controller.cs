namespace Microsoft.Examples.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;

    // note: even though this version of the controller uses attribute routing, other controllers use convention-based
    // routing. if we don't apply the ControllerName attribute, then the "controller" route parameter is not populated,
    // which will cause version 1.0 and 2.0 to not be discovered since they are convention-based.
    [ApiVersion( "3.0" )]
    [ControllerName( "Orders" )]
    [RoutePrefix( "api/orders" )]
    public class Orders3Controller : ApiController
    {
        // GET ~/orders?api-version=3.0
        [Route]
        public IHttpActionResult Get() => Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } } );

        // GET ~/orders/{id}?api-version=3.0
        [Route( "{id}" )]
        public IHttpActionResult Get( int id ) => Ok( new Order() { Id = id, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } );
    }
}