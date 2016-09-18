namespace Microsoft.AspNetCore.Mvc.ByNamespace.Controllers.V3
{
    using Models;

    [ApiVersion( "3.0" )]
    public class AgreementsController : Controller
    {
        public IActionResult Get( string accountId ) => Ok( new Agreement( GetType().FullName, accountId, HttpContext.GetRequestedApiVersion().ToString() ) );
    }
}