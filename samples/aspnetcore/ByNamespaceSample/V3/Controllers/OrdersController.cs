namespace Microsoft.Examples.V3.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [Route( "[controller]" )]
    [Route( "v{version:apiVersion}/[controller]" )]
    public class OrdersController : Controller
    {
        // GET ~/v3/orders/{accountId}
        // GET ~/orders/{accountId}?api-version=3.0
        [HttpGet( "{accountId}" )]
        public IActionResult Get( string accountId ) => Ok( new Order( GetType().FullName, accountId, HttpContext.GetRequestedApiVersion().ToString() ) );
    }
}
