namespace Microsoft.AspNetCore.Mvc.ByNamespace.Controllers.V2
{
    using Models;

    [ApiVersion( "2.0" )]
    public class AgreementsController : Controller
    {
        [HttpGet]
        public IActionResult Get( string accountId ) => Ok( new Agreement( GetType().FullName, accountId, HttpContext.GetRequestedApiVersion().ToString() ) );
    }
}