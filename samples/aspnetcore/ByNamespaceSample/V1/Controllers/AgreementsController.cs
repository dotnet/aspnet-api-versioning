namespace Microsoft.Examples.V1.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [Route( "[controller]" )]
    [Route( "v{version:apiVersion}/[controller]" )]
    public class AgreementsController : Controller
    {
        // GET ~/v1/agreements/{accountId}
        // GET ~/agreements/{accountId}?api-version=1.0
        [HttpGet( "{accountId}" )]
        public IActionResult Get( string accountId ) => Ok( new Agreement( GetType().FullName, accountId, HttpContext.GetRequestedApiVersion().ToString() ) );
    }
}