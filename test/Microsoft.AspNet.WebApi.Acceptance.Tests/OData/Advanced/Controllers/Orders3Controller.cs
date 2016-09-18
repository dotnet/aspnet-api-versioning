namespace Microsoft.Web.OData.Advanced.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;

    [ApiVersion( "3.0" )]
    [ControllerName( "Orders" )]
    [RoutePrefix( "api/orders" )]
    public class Orders3Controller : ApiController
    {
        [Route]
        public IHttpActionResult Get() => Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } } );

        [Route( "{key}" )]
        public IHttpActionResult Get( int key ) => Ok( new Order() { Id = key, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } );
    }
}