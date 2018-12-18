namespace Microsoft.Examples.Controllers
{
    using AspNetCore.Routing;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [ApiVersion( "1.0" )]
    [Route( "api/v{version:apiVersion}/[controller]" )]
    public class HelloWorldController : ControllerBase
    {
        // GET api/v{version}/helloworld
        [HttpGet]
        public IActionResult Get( ApiVersion apiVersion ) => Ok( new { Controller = GetType().Name, Version = apiVersion.ToString() } );

        // GET api/v{version}/helloworld/{id}
        [HttpGet( "{id:int}", Name = "GetMessageById" )]
        public IActionResult Get( int id, ApiVersion apiVersion ) => Ok( new { Controller = GetType().Name, Id = id, Version = apiVersion.ToString() } );

        // POST api/v{version}/helloworld
        [HttpPost]
        public IActionResult Post() => CreatedAtRoute( "GetMessageById", new { id = 42 }, null );
    }
}