namespace Microsoft.Examples.V2.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [Route( "[controller]" )]
    [Route( "v{version:apiVersion}/[controller]" )]
    public class AgreementsController : Controller
    {
        // GET ~/v2/agreements/{accountId}
        // GET ~/agreements/{accountId}?api-version=2.0
        [HttpGet( "{accountId}" )]
        public IActionResult Get( string accountId ) => Ok( new Agreement( GetType().FullName, accountId, HttpContext.GetRequestedApiVersion().ToString() ) );
    }
}