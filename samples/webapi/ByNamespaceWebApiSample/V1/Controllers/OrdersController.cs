namespace Microsoft.Examples.V1.Controllers
{
    using Microsoft.Web.Http;
    using Models;
    using System.Web.Http;

    [RoutePrefix( "v{version:apiVersion}/orders" )]
    public class OrdersController : ApiController
    {
        // GET ~/v1/orders/{accountId}
        [Route( "{accountId}" )]
        public IHttpActionResult Get( string accountId, ApiVersion apiVersion ) => Ok( new Order( GetType().FullName, accountId, apiVersion.ToString() ) );
    }
}