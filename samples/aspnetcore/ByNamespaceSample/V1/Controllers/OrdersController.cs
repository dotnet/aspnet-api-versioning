namespace Microsoft.Examples.V1.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [Route( "[controller]" )]
    [Route( "v{version:apiVersion}/[controller]" )]
    public class OrdersController : Controller
    {
        // GET ~/v1/orders/{accountId}
        // GET ~/orders/{accountId}?api-version=1.0
        [HttpGet( "{accountId}" )]
        public IActionResult Get( string accountId, ApiVersion apiVersion ) => Ok( new Order( GetType().FullName, accountId, apiVersion.ToString() ) );
    }
}