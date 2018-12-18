namespace Microsoft.Examples.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route( "api/v{version:apiVersion}/[controller]" )]
    public class HelloWorldController : ControllerBase
    {
        // GET api/v{version}/helloworld
        [HttpGet]
        public string Get( ApiVersion apiVersion ) => $"Controller = {GetType().Name}\nVersion = {apiVersion}";

        // GET api/v{version}/helloworld/{id}
        [HttpGet( "{id:int}" )]
        public string Get( int id, ApiVersion apiVersion ) => $"Controller = {GetType().Name}\nId = {id}\nVersion = {apiVersion}";
    }
}