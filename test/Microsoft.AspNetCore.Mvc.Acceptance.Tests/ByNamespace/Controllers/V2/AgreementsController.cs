namespace Microsoft.AspNetCore.Mvc.ByNamespace.Controllers.V2
{
    using Models;

    public class AgreementsController : Controller
    {
        [HttpGet]
        public IActionResult Get( string accountId ) => Ok( new Agreement( GetType().FullName, accountId, HttpContext.GetRequestedApiVersion().ToString() ) );
    }
}