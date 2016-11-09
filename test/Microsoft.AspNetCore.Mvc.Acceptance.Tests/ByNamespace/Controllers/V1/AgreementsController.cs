namespace Microsoft.AspNetCore.Mvc.ByNamespace.Controllers.V1
{
    using Models;

    [ApiVersion( "1.0" )]
    public class AgreementsController : Controller
    {
        [HttpGet]
        public IActionResult Get( string accountId ) => Ok( new Agreement( GetType().FullName, accountId, HttpContext.GetRequestedApiVersion().ToString() ) );
    }
}