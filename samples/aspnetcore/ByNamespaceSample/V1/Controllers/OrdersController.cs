namespace Microsoft.Examples.V1.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [ApiVersion( "1.0" )]
    [Route( "v{version:apiVersion}/[controller]" )]
    public class OrdersController : Controller
    {
        // GET ~/v1/orders/{accountId}
        [HttpGet( "{accountId}" )]
        public IActionResult Get( string accountId ) => Ok( new Order( GetType().FullName, accountId, HttpContext.GetRequestedApiVersion().ToString() ) );
    }
}