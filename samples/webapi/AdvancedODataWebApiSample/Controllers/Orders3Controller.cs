namespace Microsoft.Examples.Controllers
{
    using Microsoft.Examples.Models;
    using Microsoft.Web.Http;
    using System.Web.Http;

    [ApiVersion( "3.0" )]
    [ControllerName( "Orders" )]
    public class Orders3Controller : ApiController
    {
        // GET ~/orders?api-version=3.0
        public IHttpActionResult Get() => Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } } );

        // GET ~/orders/{id}?api-version=3.0
        public IHttpActionResult Get( int id ) => Ok( new Order() { Id = id, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } );
    }
}