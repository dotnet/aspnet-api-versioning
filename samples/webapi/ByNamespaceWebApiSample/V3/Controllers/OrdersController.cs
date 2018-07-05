namespace Microsoft.Examples.V3.Controllers
{
    using Models;
    using System.Web.Http;

    [RoutePrefix( "v{version:apiVersion}/[controller]" )]
    public class OrdersController : ApiController
    {
        // GET ~/v3/orders/{accountId}
        [Route( "{accountId}" )]
        public IHttpActionResult Get( string accountId ) => Ok( new Order( GetType().FullName, accountId, Request.GetRequestedApiVersion().ToString() ) );
    }
}
