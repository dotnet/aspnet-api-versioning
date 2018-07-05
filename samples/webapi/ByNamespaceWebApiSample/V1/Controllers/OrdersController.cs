namespace Microsoft.Examples.V1.Controllers
{
    using Models;
    using System.Web.Http;

    [RoutePrefix( "v{version:apiVersion}/orders" )]
    public class OrdersController : ApiController
    {
        // GET ~/v1/orders/{accountId}
        [Route( "{accountId}" )]
        public IHttpActionResult Get( string accountId ) => Ok( new Order( GetType().FullName, accountId, Request.GetRequestedApiVersion().ToString() ) );
    }
}