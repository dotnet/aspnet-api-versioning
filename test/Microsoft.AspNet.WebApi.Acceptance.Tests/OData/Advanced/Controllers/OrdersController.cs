namespace Microsoft.Web.OData.Advanced.Controllers
{
    using Models;
    using System.Web.Http;

    public class OrdersController : ApiController
    {
        public IHttpActionResult Get() => Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } } );

        public IHttpActionResult Get( int key ) => Ok( new Order() { Id = key, Customer = $"Customer v{Request.GetRequestedApiVersion()}" } );
    }
}