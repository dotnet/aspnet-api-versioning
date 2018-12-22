namespace Microsoft.AspNetCore.Mvc.MediaTypeNegotiation.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.MediaTypeNegotiation.Models;
    using Microsoft.AspNetCore.Routing;

    [ApiController]
    [Route( "api/[controller]" )]
    public class HelloWorldController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get( ApiVersion apiVersion ) => Ok( new { Controller = nameof( HelloWorldController ), Version = apiVersion.ToString() } );

        [HttpGet( "{id:int}" )]
        public IActionResult Get( int id, ApiVersion apiVersion ) => Ok( new { Controller = GetType().Name, Id = id, Version = apiVersion.ToString() } );

        [HttpPost]
        public IActionResult Post( Message message, ApiVersion apiVersion ) => CreatedAtAction( nameof( Get ), new { id = 42 }, message );
    }
}