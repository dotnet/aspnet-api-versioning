namespace Microsoft.Examples.V2.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [ApiController]
    [Route( "[controller]" )]
    [Route( "v{version:apiVersion}/[controller]" )]
    public class AgreementsController : ControllerBase
    {
        // GET ~/v2/agreements/{accountId}
        // GET ~/agreements/{accountId}?api-version=2.0
        [HttpGet( "{accountId}" )]
        public IActionResult Get( string accountId, ApiVersion apiVersion ) => Ok( new Agreement( GetType().FullName, accountId, apiVersion.ToString() ) );
    }
}