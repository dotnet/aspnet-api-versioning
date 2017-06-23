namespace Microsoft.AspNetCore.Mvc.Basic.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;

    [ApiVersion( "1.0" )]
    [Route( "api/v{version:apiVersion}/values" )]
    public class OverlappingRouteTemplateController : Controller
    {
        [HttpGet( "{id:int}/{childId}" )]
        public IActionResult Get( int id, string childId ) => Ok( new { id, childId } );

        [HttpGet( "{id:int}/children" )]
        public IActionResult Get( int id ) => Ok( new { id } );

        [HttpGet( "{id:int}/ambiguous" )]
        public IActionResult Ambiguous( int id ) => Ok();

        [HttpGet( "{id:int}/ambiguous" )]
        public IActionResult Ambiguous2( int id ) => Ok();
    }
}