namespace Microsoft.Examples.V3.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [ApiController]
    [Route( "[controller]" )]
    [Route( "v{version:apiVersion}/[controller]" )]
    public class AgreementsController : ControllerBase
    {
        // GET ~/v3/agreements/{accountId}
        // GET ~/agreements/{accountId}?api-version=3.0
        [HttpGet( "{accountId}" )]
        public IActionResult Get( string accountId, ApiVersion apiVersion ) => Ok( new Agreement( GetType().FullName, accountId, apiVersion.ToString() ) );
    }
}