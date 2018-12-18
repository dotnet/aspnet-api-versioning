namespace Microsoft.AspNetCore.Mvc.ByNamespace.Controllers.V3
{
    using Models;

    [ApiController]
    public class AgreementsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get( string accountId, ApiVersion apiVersion ) => Ok( new Agreement( GetType().FullName, accountId, apiVersion.ToString() ) );
    }
}