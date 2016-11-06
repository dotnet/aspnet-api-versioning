namespace Microsoft.Web.OData.Advanced.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;

    [ApiVersion( "3.0" )]
    [ControllerName( "Orders" )]
    public class Orders3Controller : ApiController
    {
        public IHttpActionResult Get() => Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } } );

        public IHttpActionResult Get( int key ) => Ok( new Order() { Id = key, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } );
    }
}